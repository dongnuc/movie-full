using Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Org.BouncyCastle.Asn1.Ess;
using StackExchange.Redis;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Application.Abstractions.Services.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Movie_StructureCode.Application.Abstractions.Services.SeatService
{
    /// <summary>
    /// Seat Lock Service using Redis NX (SET if Not eXists) and SQL DB fallback.
    /// Excludes hold seats from ZSET which is reserved for booked seats.
    /// </summary>
    public class SeatLockService : ISeatLockService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IBookingSessionRepository _bookingSessionRepository;
        private readonly IShowingSeatRepository _showingSeatRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISeatHubService _seatHubService;

        private readonly string _lockPrefix = "MovieApp:seat:lock:"; // source of truth for lock ownership and TTL
        private readonly string _userLockPrefix = "MovieApp:seat:user:"; // track which seats a user is holding for easy query and management
        private readonly string _sessionUser = "MovieApp:seat:session:"; // track user session for lock management and TTL
        private readonly string _sessionWarning = "MovieApp:seat:session-warning:"; // track warning event TTL for user session
        private readonly string _avaiableSeats = "MovieApp:seat:showing:seatAvaiable:"; // cache available seats for showing

        private const int DEFAULT_LOCK_DURATION_SECONDS = 300; // 5 minutes

        private string GetSessionKey(Guid showingId, string userId)
            => $"{_sessionUser}{showingId}:{userId}";

        private string GetSessionWarningKey(Guid showingId, string userId)
            => $"{_sessionWarning}{showingId}:{userId}";

        private string GetUserKey(Guid showingId, string userId)
            => $"{_userLockPrefix}{showingId}:{userId}";

        // Lua script for atomic lock operation (ZSET completely removed)
        private const string LUA_MULTI_LOCK = @"
            -- KEYS: [1..N] = seat lock keys
            -- ARGV: [1] = userId, [2] = ttlSeconds

            local seatCount = #KEYS

            -- 1. Check conflict
            for i=1,seatCount do
                local current = redis.call('GET', KEYS[i])
                if current and current ~= ARGV[1] then
                    return 0
                end
            end

            -- 2. Set lock
            local ttl = tonumber(ARGV[2])

            for i=1,seatCount do
                redis.call('SET', KEYS[i], ARGV[1], 'EX', ttl)
            end

            return 1
            ";

        // Lua script for atomic unlock operation (ZSET completely removed)
        private const string LUA_MULTI_UNLOCK = @"
            -- KEYS: [1..N] = seat lock keys, [N+1] = userKey
            -- ARGV: [1] = userId, [2..] = seatIds

            local userId = ARGV[1]
            local seatCount = #KEYS - 1

            local userKey = KEYS[seatCount + 1]

            for i = 1, seatCount do
                local lockKey = KEYS[i]
                local seatId = ARGV[i + 1]

                local current = redis.call('GET', lockKey)

                -- chỉ xoá nếu đúng owner
                if current and current == userId then
                    redis.call('DEL', lockKey)
                    redis.call('SREM', userKey, seatId)
                end
            end

            return 1
            ";

        // Lua script for changing status and extending lock TTL (ZSET completely removed)
        private const string LUA_CHANGE_STATUS = @"
            -- KEYS: 1 = sessionKey, 2 = userKey
            -- ARGV: 1 = userId, 2 = extendSeconds, 3 = showingId

            local sessionKey = KEYS[1]
            local userKey = KEYS[2]

            -- 1. check session tồn tại
            if redis.call('EXISTS', sessionKey) == 0 then
                return 0
            end

            -- 2. check status
            local status = redis.call('HGET', sessionKey, 'status')
            if status ~= 'HOLDING' and status ~= 'select' and status ~= 'PAYING' then
                return 0
            end

            -- 3. check TTL
            local ttl = redis.call('TTL', sessionKey)
            if ttl <= 0 then
                return 0
            end

            -- 4. check extended
            local extended = redis.call('HGET', sessionKey, 'extended')

            local newTtl = ttl

            if extended == '0' then
                newTtl = ttl + tonumber(ARGV[2])

                redis.call('HSET', sessionKey,
                    'status', 'PAYING',
                    'extended', '1'
                )
            end

            -- 5. update TTL session + user
            redis.call('EXPIRE', sessionKey, newTtl)
            redis.call('EXPIRE', userKey, newTtl)

            -- 6. update TTL cho tất cả seat lock
            local seats = redis.call('SMEMBERS', userKey)

            for i=1,#seats do
                local seatId = seats[i]
                local lockKey = 'MovieApp:seat:lock:' .. ARGV[3] .. ':' .. seatId
                redis.call('EXPIRE', lockKey, newTtl)
            end

            -- 7. update warning key TTL
            local warningKey = 'MovieApp:seat:session-warning:' .. ARGV[3] .. ':' .. ARGV[1]
            if newTtl > 60 then
                redis.call('SET', warningKey, '1', 'EX', newTtl - 60)
            else
                redis.call('DEL', warningKey)
            end

            return 1";

        public SeatLockService(
            IConnectionMultiplexer redis,
            IBookingSessionRepository bookingSessionRepository,
            IShowingSeatRepository showingSeatRepository,
            IUnitOfWork unitOfWork,
            ISeatHubService seatHubService)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _bookingSessionRepository = bookingSessionRepository ?? throw new ArgumentNullException(nameof(bookingSessionRepository));
            _showingSeatRepository = showingSeatRepository ?? throw new ArgumentNullException(nameof(showingSeatRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _seatHubService = seatHubService ?? throw new ArgumentNullException(nameof(seatHubService));
        }

        /// <summary>
        /// Lock multiple seats atomically for a user
        /// </summary>
        public async Task<(bool Success, Guid? SessionId, int? RemainingSeconds)> LockSeatsAsync(Guid showingId, List<string> seatIds, string userId)
        {
            if (!seatIds.Any()) return (true, null, null);

            var userIdGuid = Guid.Parse(userId);
            var now = DateTime.UtcNow;

            var db = _redis.GetDatabase();
            var sessionKey = GetSessionKey(showingId, userId);

            bool hasRedisSession = false;
            TimeSpan redisTtl = TimeSpan.Zero;
            try
            {
                var ttl = await db.KeyTimeToLiveAsync(sessionKey);
                if (ttl != null && ttl > TimeSpan.Zero)
                {
                    hasRedisSession = true;
                    redisTtl = ttl.Value;
                }
            }
            catch (Exception)
            {
                // Redis offline: ignore
            }

            BookingSession? activeSession = await _bookingSessionRepository.GetActiveSessionAsync(showingId, userIdGuid);

            TimeSpan remainingTtl = TimeSpan.FromSeconds(DEFAULT_LOCK_DURATION_SECONDS);

            if (hasRedisSession)
            {
                remainingTtl = redisTtl;
            }
            else if (activeSession != null)
            {
                remainingTtl = activeSession.GracePeriodEndsAt - now;
            }

            if (remainingTtl <= TimeSpan.Zero)
            {
                return (false, null, null); // Expired
            }

            // If remaining time is under 60s before timeout, push warning message
            if (remainingTtl.TotalSeconds < 60)
            {
                try
                {
                    await _seatHubService.NotifySessionTimeoutWarningAsync(showingId, userId, (int)Math.Ceiling(remainingTtl.TotalSeconds));
                }
                catch (Exception)
                {
                    // SignalR offline: ignore
                }
            }

            // 1. Try to acquire lock in Redis first (atomic concurrency guard)
            bool isRedisOnline = false;
            try
            {
                var ttlSeconds = (int)Math.Ceiling(remainingTtl.TotalSeconds);
                var seatLockKeys = seatIds.Select(seatId => (RedisKey)$"{_lockPrefix}{showingId}:{seatId}").ToArray();

                var args = new RedisValue[]
                {
                    userId,
                    ttlSeconds
                };

                var redisResult = await db.ScriptEvaluateAsync(LUA_MULTI_LOCK, seatLockKeys, args);
                isRedisOnline = true;

                if (redisResult.IsNull || (long)redisResult == 0)
                {
                    // Redis lock check failed (someone else holds the lock)
                    return (false, null, null);
                }
            }
            catch (Exception)
            {
                // Redis offline: fallback to DB-only synchronization
                isRedisOnline = false;
            }

            // 2. Conflict check in DB
            var lockedSeatsInDb = await _bookingSessionRepository.GetLockedSeatIdsForShowingAsync(showingId);
            var mySeatIds = activeSession?.BookingSessionSeats?.Select(s => s.SeatId).ToHashSet() ?? new HashSet<Guid>();
            bool dbConflict = false;

            foreach (var seatIdStr in seatIds)
            {
                if (Guid.TryParse(seatIdStr, out var seatIdGuid))
                {
                    if (lockedSeatsInDb.Contains(seatIdGuid) && !mySeatIds.Contains(seatIdGuid))
                    {
                        dbConflict = true;
                        break;
                    }
                }
            }

            if (dbConflict)
            {
                // Release Redis locks if we acquired them to keep cache in sync with DB reject
                if (isRedisOnline)
                {
                    try
                    {
                        var seatLockKeys = seatIds.Select(seatId => (RedisKey)$"{_lockPrefix}{showingId}:{seatId}").ToArray();
                        var userKey = (RedisKey)GetUserKey(showingId, userId);
                        var keys = seatLockKeys.Append(userKey).ToArray();
                        var unlockArgs = new List<RedisValue> { userId };
                        unlockArgs.AddRange(seatIds.Select(s => (RedisValue)s));
                        await db.ScriptEvaluateAsync(LUA_MULTI_UNLOCK, keys, unlockArgs.ToArray());
                    }
                    catch (Exception)
                    {
                        // Ignore redis cleanup error
                    }
                }
                return (false, null, null);
            }

            // 3. Update Database (Create or extend session)
            bool isNewSession = false;
            bool isFirstSeatInSession = false;

            try
            {
                if (activeSession == null)
                {
                    activeSession = BookingSession.Create(Guid.NewGuid(), showingId, userIdGuid, "select", 5);
                    
                    // Add seats to session
                    foreach (var seatIdStr in seatIds)
                    {
                        if (Guid.TryParse(seatIdStr, out var seatIdGuid))
                        {
                            activeSession.BookingSessionSeats.Add(new BookingSessionSeat(activeSession.Id, seatIdGuid));
                        }
                    }

                    await _bookingSessionRepository.AddAsync(activeSession);
                    await _unitOfWork.SaveChangesAsync();
                    isNewSession = true;
                    isFirstSeatInSession = true;
                }
                else
                {
                    activeSession.Reconnect();

                    if (activeSession.BookingSessionSeats == null || !activeSession.BookingSessionSeats.Any())
                    {
                        isFirstSeatInSession = true;
                    }

                    var existingSeatIds = activeSession.BookingSessionSeats.Select(s => s.SeatId).ToHashSet();
                    bool dbModified = false;
                    foreach (var seatIdStr in seatIds)
                    {
                        if (Guid.TryParse(seatIdStr, out var seatIdGuid))
                        {
                            if (!existingSeatIds.Contains(seatIdGuid))
                            {
                                activeSession.BookingSessionSeats.Add(new BookingSessionSeat(activeSession.Id, seatIdGuid));
                                dbModified = true;
                            }
                        }
                    }

                    if (dbModified)
                    {
                        _bookingSessionRepository.Update(activeSession);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {
                // Release Redis locks if we acquired them to prevent deadlock
                if (isRedisOnline)
                {
                    try
                    {
                        var seatLockKeys = seatIds.Select(seatId => (RedisKey)$"{_lockPrefix}{showingId}:{seatId}").ToArray();
                        var userKey = (RedisKey)GetUserKey(showingId, userId);
                        var keys = seatLockKeys.Append(userKey).ToArray();
                        var unlockArgs = new List<RedisValue> { userId };
                        unlockArgs.AddRange(seatIds.Select(s => (RedisValue)s));
                        await db.ScriptEvaluateAsync(LUA_MULTI_UNLOCK, keys, unlockArgs.ToArray());
                    }
                    catch (Exception)
                    {
                        // Ignore
                    }
                }
                throw;
            }

            // 4. Synchronize with Redis structures (if Redis is online)
            if (isRedisOnline)
            {
                try
                {
                    var userKey = GetUserKey(showingId, userId);
                    await db.SetAddAsync(userKey, seatIds.Select(s => (RedisValue)s).ToArray());
                    await db.KeyExpireAsync(userKey, remainingTtl);

                    if (!hasRedisSession)
                    {
                        await db.HashSetAsync(sessionKey, new HashEntry[]
                        {
                            new("status", activeSession?.Status ?? "select"),
                            new("extended", activeSession != null && activeSession.Extend ? "1" : "0"),
                            new("createdAt", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                        });
                        await db.KeyExpireAsync(sessionKey, remainingTtl);
                    }
                    else
                    {
                        await db.KeyExpireAsync(sessionKey, remainingTtl);
                    }

                    var warningKey = GetSessionWarningKey(showingId, userId);
                    if (remainingTtl.TotalSeconds > 60)
                    {
                        await db.StringSetAsync(warningKey, "1", TimeSpan.FromSeconds(remainingTtl.TotalSeconds - 60));
                    }
                    else
                    {
                        await db.KeyDeleteAsync(warningKey);
                    }
                }
                catch (Exception)
                {
                    // Ignore cache update errors
                }
            }

            var shouldReturnSessionTtl = isNewSession || isFirstSeatInSession;
            return (true, shouldReturnSessionTtl ? activeSession?.Id : null, shouldReturnSessionTtl ? (int?)Math.Ceiling(remainingTtl.TotalSeconds) : null);
        }

        /// <summary>
        /// Unlock multiple seats atomically
        /// </summary>
        public async Task<bool> UnlockSeatsAsync(Guid showingId, List<string> seatIds, string userId)
        {
            if (!seatIds.Any()) return true;

            var userIdGuid = Guid.Parse(userId);

            // DB update: remove specified seats from current active session
            var activeSession = await _bookingSessionRepository.GetActiveSessionAsync(showingId, userIdGuid);
            if (activeSession != null)
            {
                if (activeSession.BookingSessionSeats != null)
                {
                    var seatsToRemove = activeSession.BookingSessionSeats
                        .Where(s => seatIds.Contains(s.SeatId.ToString()))
                        .ToList();

                    foreach (var s in seatsToRemove)
                    {
                        activeSession.BookingSessionSeats.Remove(s);
                    }
                    _bookingSessionRepository.Update(activeSession);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            // Redis update: remove seat locks
            try
            {
                var db = _redis.GetDatabase();
                var seatKeys = seatIds
                    .Select(seatId => (RedisKey)$"{_lockPrefix}{showingId}:{seatId}")
                    .ToList();

                var userKey = (RedisKey)GetUserKey(showingId, userId);

                // Keys list for Lua script: seat lock keys + user key
                var keys = seatKeys.Append(userKey).ToArray();

                var args = new List<RedisValue> { userId };
                args.AddRange(seatIds.Select(s => (RedisValue)s));

                await db.ScriptEvaluateAsync(
                    LUA_MULTI_UNLOCK,
                    keys,
                    args.ToArray());
            }
            catch (Exception)
            {
                // Redis offline: ignore
            }

            return true;
        }

        public async Task<List<string>> GetUserLockedSeatsAsync(Guid showingId, string userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var userKey = GetUserKey(showingId, userId);
                var values = await db.SetMembersAsync(userKey);
                if (values != null && values.Length > 0)
                {
                    return values.Select(x => x.ToString()).ToList();
                }
            }
            catch (Exception)
            {
                // Redis offline: fallback to DB
            }

            var userIdGuid = Guid.Parse(userId);
            var activeSession = await _bookingSessionRepository.GetActiveSessionAsync(showingId, userIdGuid);
            if (activeSession?.BookingSessionSeats != null)
            {
                return activeSession.BookingSessionSeats.Select(s => s.SeatId.ToString()).ToList();
            }

            return new List<string>();
        }

        /// <summary>
        /// Count locked seats in a showing
        /// </summary>
        public async Task<int> CountLockedSeatsAsync(Guid showingId)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: $"{_lockPrefix}{showingId}:*");
                return keys.Count();
            }
            catch (Exception)
            {
                // Redis offline: fallback to DB
            }

            var lockedSeats = await _bookingSessionRepository.GetLockedSeatIdsForShowingAsync(showingId);
            return lockedSeats.Count;
        }

        public async Task<(List<string> locked, List<string> unlocked, List<string> failed)>
           SyncUserSeatsAsync(Guid showingId, List<string> requestSeats, string userId)
        {
            var current = await GetUserLockedSeatsAsync(showingId, userId);

            var toAdd = requestSeats.Except(current).ToList();
            var toRemove = current.Except(requestSeats).ToList();

            var locked = new List<string>();
            var failed = new List<string>();

            if (toAdd.Any())
            {
                foreach (var seatId in toAdd)
                {
                    var (success, _, _) = await LockSeatsAsync(showingId, new List<string> { seatId }, userId);
                    if (success)
                        locked.Add(seatId);
                    else
                        failed.Add(seatId);
                }
            }

            if (toRemove.Any())
            {
                await UnlockSeatsAsync(showingId, toRemove, userId);
            }

            return (locked, toRemove, failed);
        }

        /// <summary>
        /// Lấy toàn bộ seat đang hold của tất cả user theo showing (kết hợp cả Redis và DB)
        /// </summary>
        public async Task<List<Guid>> GetAllLockedSeatsByShowingAsync(Guid showingId)
        {
            var result = new HashSet<Guid>();

            // 1. Lấy từ Redis
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: $"{_lockPrefix}{showingId}:*").ToList();
                foreach (var key in keys)
                {
                    var parts = key.ToString().Split(':');
                    if (parts.Length >= 4 && Guid.TryParse(parts[3], out var seatGuid))
                    {
                        result.Add(seatGuid);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Offline] GetAllLockedSeatsByShowingAsync failed to read Redis: {ex.Message}. Relies on DB fallback.");
            }

            // 2. Luôn kết hợp với các session đang active trong DB để đảm bảo tính nhất quán tuyệt đối
            var lockedSeatsDb = await _bookingSessionRepository.GetLockedSeatIdsForShowingAsync(showingId);
            foreach (var seatId in lockedSeatsDb)
            {
                result.Add(seatId);
            }

            return result.ToList();
        }

        /// <summary>
        /// Lấy danh sách user đang hold ghế theo showingId và userId (kết hợp cả Redis và DB)
        /// </summary>
        public async Task<HashSet<Guid>> GetUserHoldingSeatsAsync(Guid showingId, string userId)
        {
            var result = new HashSet<Guid>();

            // 1. Lấy từ Redis
            try
            {
                var db = _redis.GetDatabase();
                var userKey = GetUserKey(showingId, userId);
                var seatValues = await db.SetMembersAsync(userKey);

                if (seatValues != null && seatValues.Length > 0)
                {
                    var staleSeats = new List<RedisValue>();

                    foreach (var seatValue in seatValues)
                    {
                        var seatId = seatValue.ToString();
                        var lockKey = $"{_lockPrefix}{showingId}:{seatId}";
                        var exists = await db.KeyExistsAsync(lockKey);

                        if (exists)
                        {
                            result.Add(Guid.Parse(seatId!));
                        }
                        else
                        {
                            staleSeats.Add(seatValue);
                        }
                    }

                    if (staleSeats.Count > 0)
                    {
                        await db.SetRemoveAsync(userKey, staleSeats.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Offline] GetUserHoldingSeatsAsync failed to read Redis: {ex.Message}. Relies on DB fallback.");
            }

            // 2. Luôn kết hợp với active session trong DB để đảm bảo tính nhất quán tuyệt đối
            var userIdGuid = Guid.Parse(userId);
            var activeSession = await _bookingSessionRepository.GetActiveSessionAsync(showingId, userIdGuid);
            if (activeSession?.BookingSessionSeats != null)
            {
                foreach (var bss in activeSession.BookingSessionSeats)
                {
                    result.Add(bss.SeatId);
                }
            }

            return result;
        }

        public async Task<int?> MarkSessionAsPayingAsync(Guid showingId, string userId)
        {
            var userIdGuid = Guid.Parse(userId);

            var activeSession = await _bookingSessionRepository.GetActiveSessionAsync(showingId, userIdGuid);
            bool dbSuccess = false;
            TimeSpan newTtl = TimeSpan.Zero;

            if (activeSession != null)
            {
                var now = DateTime.UtcNow;
                var remaining = activeSession.GracePeriodEndsAt - now;
                if (remaining > TimeSpan.Zero)
                {
                    if (activeSession.Extend)
                    {
                        dbSuccess = true;
                        newTtl = remaining;
                    }
                    else
                    {
                        dbSuccess = activeSession.TryExtendSession(2);
                        if (dbSuccess)
                        {
                            _bookingSessionRepository.Update(activeSession);
                            await _unitOfWork.SaveChangesAsync();
                            newTtl = activeSession.GracePeriodEndsAt - now;
                        }
                    }
                }
            }

            try
            {
                var db = _redis.GetDatabase();
                var sessionKey = GetSessionKey(showingId, userId);
                var userKey = GetUserKey(showingId, userId);

                var key = new RedisKey[]
                {
                    sessionKey,
                    userKey
                };

                var result = (long)await db.ScriptEvaluateAsync(
                    LUA_CHANGE_STATUS,
                    key,
                    new RedisValue[]
                    {
                        userId,
                        120,
                        showingId.ToString()
                    });

                if (result == 1)
                {
                    var ttl = await db.KeyTimeToLiveAsync(sessionKey);
                    if (ttl.HasValue && ttl.Value > TimeSpan.Zero)
                    {
                        return (int)Math.Ceiling(ttl.Value.TotalSeconds);
                    }
                }

                if (dbSuccess && newTtl > TimeSpan.Zero)
                {
                    return (int)Math.Ceiling(newTtl.TotalSeconds);
                }

                return null;
            }
            catch (Exception)
            {
                if (dbSuccess && newTtl > TimeSpan.Zero)
                {
                    return (int)Math.Ceiling(newTtl.TotalSeconds);
                }
                return null;
            }
        }

        public async Task<int?> GetSessionRemainingSeconds(Guid showingId, string userId)
        {
            try
            {
                var db = _redis.GetDatabase();
                var sessionKey = GetSessionKey(showingId, userId);
                var ttl = await db.KeyTimeToLiveAsync(sessionKey);

                if (ttl != null && ttl > TimeSpan.Zero)
                {
                    return (int)Math.Ceiling(ttl.Value.TotalSeconds);
                }
            }
            catch (Exception)
            {
                // Redis offline: fallback to DB
            }

            var userIdGuid = Guid.Parse(userId);
            var activeSession = await _bookingSessionRepository.GetActiveSessionAsync(showingId, userIdGuid);
            if (activeSession != null)
            {
                var now = DateTime.UtcNow;
                var diff = activeSession.GracePeriodEndsAt - now;
                if (diff > TimeSpan.Zero)
                {
                    return (int)Math.Ceiling(diff.TotalSeconds);
                }
            }

            return null;
        }

        public async Task<HashSet<Guid>> GetBookedSeatsAsync(Guid showingId, CancellationToken cancellationToken = default)
        {
            var zsetKey = $"MovieApp:seat:locks:zset:{showingId}";
            try
            {
                var db = _redis.GetDatabase();
                var exists = await db.KeyExistsAsync(zsetKey);
                if (exists)
                {
                    var bookedMembers = await db.SortedSetRangeByRankAsync(zsetKey);
                    var result = new HashSet<Guid>();
                    if (bookedMembers != null)
                    {
                        foreach (var member in bookedMembers)
                        {
                            if (Guid.TryParse(member.ToString(), out var seatGuid))
                            {
                                result.Add(seatGuid);
                            }
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Offline] Failed to check booked seats existence from ZSET: {ex.Message}. Falling back to DB.");
            }

            // Fallback DB
            var showingSeats = await _showingSeatRepository.GetShowingSeatsWithBooking(showingId, cancellationToken);
            var dbBookedSeats = showingSeats
                .Where(ss => ss.Status == ShowingSeatStatus.Booked)
                .Select(ss => ss.SeatId)
                .ToHashSet();

            // Warm up Redis ZSET in background
            try
            {
                var db = _redis.GetDatabase();
                if (dbBookedSeats.Any())
                {
                    var entries = dbBookedSeats.Select(guid => new SortedSetEntry(guid.ToString(), 0)).ToArray();
                    await db.SortedSetAddAsync(zsetKey, entries);
                }
                else
                {
                    // Add dummy placeholder to prevent cache penetration
                    await db.SortedSetAddAsync(zsetKey, "empty_placeholder", -1);
                }
                await db.KeyExpireAsync(zsetKey, TimeSpan.FromHours(2));
            }
            catch (Exception)
            {
                // Ignore warm up write errors
            }

            return dbBookedSeats;
        }

        public async Task ClearSessionAfterBookingAsync(Guid showingId, List<string> seatIds, string userId)
        {
            var userIdGuid = Guid.Parse(userId);

            // 1. DB: Remove active booking session
            try
            {
                var activeSession = await _bookingSessionRepository.GetActiveSessionAsync(showingId, userIdGuid);
                if (activeSession != null)
                {
                    _bookingSessionRepository.Remove(activeSession);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB Error] ClearSessionAfterBookingAsync failed to remove session: {ex.Message}");
            }

            // 2. Redis: Clear session, locks, and update ZSET
            try
            {
                var db = _redis.GetDatabase();
                var sessionKey = GetSessionKey(showingId, userId);
                var userKey = GetUserKey(showingId, userId);
                var warningKey = GetSessionWarningKey(showingId, userId);
                var zsetKey = $"MovieApp:seat:locks:zset:{showingId}";

                // Delete keys
                var keysToDelete = new List<RedisKey> { sessionKey, userKey, warningKey };
                foreach (var seatId in seatIds)
                {
                    keysToDelete.Add($"{_lockPrefix}{showingId}:{seatId}");
                }
                await db.KeyDeleteAsync(keysToDelete.ToArray());

                // Add to booked ZSET
                if (seatIds.Any())
                {
                    var exists = await db.KeyExistsAsync(zsetKey);
                    if (exists)
                    {
                        await db.SortedSetRemoveAsync(zsetKey, "empty_placeholder");
                    }

                    var entries = seatIds.Select(seatId => new SortedSetEntry(seatId, 0)).ToArray();
                    await db.SortedSetAddAsync(zsetKey, entries);
                    await db.KeyExpireAsync(zsetKey, TimeSpan.FromHours(2));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Error] ClearSessionAfterBookingAsync failed: {ex.Message}. Suppressing to allow DB transaction to succeed.");
            }
        }
    }
}
