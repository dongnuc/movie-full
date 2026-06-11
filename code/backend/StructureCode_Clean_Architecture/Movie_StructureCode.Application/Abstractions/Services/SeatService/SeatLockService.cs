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
        private readonly string _avaiableSeats = "MovieApp:seat:showing:seatAvaiable:"; // cache available seats for showing

        private const int DEFAULT_LOCK_DURATION_SECONDS = 300; // 5 minutes

        private string GetSessionKey(Guid showingId, string userId)
            => $"{_sessionUser}{showingId}:{userId}";

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
            if status ~= 'HOLDING' and status ~= 'select' then
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
        public async Task<bool> LockSeatsAsync(Guid showingId, List<string> seatIds, string userId)
        {
            if (!seatIds.Any()) return true;

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

            BookingSession? activeSession = null;
            if (!hasRedisSession)
            {
                activeSession = await _bookingSessionRepository.GetActiveSessionAsync(showingId, userIdGuid);
            }

            TimeSpan remainingTtl = TimeSpan.FromSeconds(DEFAULT_LOCK_DURATION_SECONDS);

            if (!hasRedisSession && activeSession == null)
            {
                activeSession = BookingSession.Create(Guid.NewGuid(), showingId, userIdGuid, "select", 5);
                await _bookingSessionRepository.AddAsync(activeSession);
                await _unitOfWork.SaveChangesAsync();
                remainingTtl = TimeSpan.FromSeconds(DEFAULT_LOCK_DURATION_SECONDS);
            }
            else
            {
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
                    return false; // Expired
                }

                // If remaining time is under 30s before timeout, push warning message
                if (remainingTtl.TotalSeconds < 30)
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
            }

            // Conflict check
            // DB check
            var lockedSeatsInDb = await _bookingSessionRepository.GetLockedSeatIdsForShowingAsync(showingId);
            if (activeSession == null)
            {
                activeSession = await _bookingSessionRepository.GetActiveSessionAsync(showingId, userIdGuid);
            }

            var mySeatIds = activeSession?.BookingSessionSeats?.Select(s => s.SeatId).ToHashSet() ?? new HashSet<Guid>();
            foreach (var seatIdStr in seatIds)
            {
                if (Guid.TryParse(seatIdStr, out var seatIdGuid))
                {
                    if (lockedSeatsInDb.Contains(seatIdGuid) && !mySeatIds.Contains(seatIdGuid))
                    {
                        return false; // Conflicting lock in DB
                    }
                }
            }

            // Redis check (if Redis is online)
            try
            {
                foreach (var seatId in seatIds)
                {
                    var lockKey = $"{_lockPrefix}{showingId}:{seatId}";
                    var owner = await db.StringGetAsync(lockKey);
                    if (owner.HasValue && owner != userId)
                    {
                        return false; // Conflicting lock in Redis cache
                    }
                }
            }
            catch (Exception)
            {
                // Redis offline: fallback to DB check
            }

            // Update database
            if (activeSession != null)
            {
                activeSession.Reconnect();

                var existingSeatIds = activeSession.BookingSessionSeats.Select(s => s.SeatId).ToHashSet();
                foreach (var seatIdStr in seatIds)
                {
                    if (Guid.TryParse(seatIdStr, out var seatIdGuid))
                    {
                        if (!existingSeatIds.Contains(seatIdGuid))
                        {
                            activeSession.BookingSessionSeats.Add(new BookingSessionSeat(activeSession.Id, seatIdGuid));
                        }
                    }
                }
                _bookingSessionRepository.Update(activeSession);
                await _unitOfWork.SaveChangesAsync();
            }

            // Synchronize with Redis cache (without ZSET)
            try
            {
                var ttlSeconds = (int)Math.Ceiling(remainingTtl.TotalSeconds);
                var seatLockKeys = seatIds.Select(seatId => (RedisKey)$"{_lockPrefix}{showingId}:{seatId}").ToArray();

                var args = new RedisValue[]
                {
                    userId,
                    ttlSeconds
                };

                await db.ScriptEvaluateAsync(LUA_MULTI_LOCK, seatLockKeys, args);

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
            }
            catch (Exception)
            {
                // Redis offline: ignore cache update errors
            }

            return true;
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
                    var success = await LockSeatsAsync(showingId, new List<string> { seatId }, userId);
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

        public async Task<bool> MarkSessionAsPayingAsync(Guid showingId, string userId)
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
                    dbSuccess = activeSession.TryExtendSession(2);
                    if (dbSuccess)
                    {
                        _bookingSessionRepository.Update(activeSession);
                        await _unitOfWork.SaveChangesAsync();
                        newTtl = activeSession.GracePeriodEndsAt - now;
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

                return result == 1 || dbSuccess;
            }
            catch (Exception)
            {
                return dbSuccess;
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
    }
}
