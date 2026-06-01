using Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat;
using Movie_StructureCode.Contract.Abstractions.Shared;
using Org.BouncyCastle.Asn1.Ess;
using StackExchange.Redis;

namespace Movie_StructureCode.Application.Abstractions.Services.SeatService
{
    /// <summary>
    /// Seat Lock Service using Redis NX (SET if Not eXists) and Lua Scripts
    /// Lock mechanism: Non-blocking, atomic operations
    /// </summary>
    public class SeatLockService : ISeatLockService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly string _lockPrefix = "seat:lock:"; // source of truth for lock ownership and TTL
        private readonly string _userLockPrefix = "seat:user:"; // track which seats a user is holding for easy query and management
        private readonly string _zsetPrefix = "seat:locks:zset:"; // support expire time tracking and querying expiring locks
        private readonly string _sessionUser = "seat:session:"; // track user session for lock management and TTL
        private readonly string _avaiableSeats = "seat:showing:seatAvaiable:"; // cache available seats for showing (optional, can be used for optimization)

        private const int DEFAULT_LOCK_DURATION_SECONDS = 300; // 5 minutes


        private string GetSessionKey(Guid showingId, string userId)
        => $"{_sessionUser}{showingId}:{userId}";

        private string GetUserKey(Guid showingId, string userId)
            => $"{_userLockPrefix}{showingId}:{userId}";


        // Lua script for atomic lock operation
        // add lock + expire time vào zset để dễ dàng query các lock sắp hết hạn
        private const string LUA_MULTI_LOCK = @"
            -- KEYS: [1..N] = seat lock keys, [N+1] = zset key
            -- ARGV: [1] = userId, [2] = ttlSeconds, [3] = nowTimestamp, [4..] = seatIds

            local seatCount = #KEYS - 1

            -- 1. Check conflict
            for i=1,seatCount do
                local current = redis.call('GET', KEYS[i])
                if current and current ~= ARGV[1] then
                    return 0
                end
            end

            -- 2. Set lock + add to ZSET
            local ttl = tonumber(ARGV[2])
            local now = tonumber(ARGV[3])
            local expireAt = now + ttl

            for i=1,seatCount do
                redis.call('SET', KEYS[i], ARGV[1], 'EX', ttl)
                redis.call('ZADD', KEYS[seatCount + 1], expireAt, ARGV[3 + i])
            end

            return 1
            ";

        // Lua script for atomic unlock operation
        private const string LUA_MULTI_UNLOCK = @"
                       -- KEYS: [1..N] = seat lock keys, [N+1] = userKey, [N+2] = zsetKey
            -- ARGV: [1] = userId, [2..] = seatIds

            local userId = ARGV[1]
            local seatCount = #KEYS - 2

            local userKey = KEYS[seatCount + 1]
            local zsetKey = KEYS[seatCount + 2]

            for i = 1, seatCount do
                local lockKey = KEYS[i]
                local seatId = ARGV[i + 1]

                local current = redis.call('GET', lockKey)

                -- chỉ xoá nếu đúng owner
                if current and current == userId then
                    redis.call('DEL', lockKey)
                    redis.call('ZREM', zsetKey, seatId)
                    redis.call('SREM', userKey, seatId)
                end
            end

            return 1
            ";

        private const string LUA_CHANGE_STATUS = @"
            -- KEYS: 1 = sessionKey, 2 = userKey, 3 = zsetKey, 4..N = seat lock keys
            -- ARGV: 1 = userId, 2 = extendSeconds, 3 = now

            local sessionKey = KEYS[1]
            local userKey = KEYS[2]
            local zsetKey = KEYS[3]

            -- 1. check session tồn tại
            if redis.call('EXISTS', sessionKey) == 0 then
                return 0
            end

            -- 2. check status
            local status = redis.call('HGET', sessionKey, 'status')
            if status ~= 'HOLDING' then
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
                local lockKey = 'seat:lock:' .. ARGV[1] .. ':' .. seatId
                redis.call('EXPIRE', lockKey, newTtl)

                -- update expireAt trong ZSET
                redis.call('ZADD', zsetKey, ARGV[3] + newTtl, seatId)
            end

            return 1";

        private async Task<TimeSpan> GetOrCreateSessionTtlAsync(IDatabase db,string sessionKey)
        {
            var ttl = await db.KeyTimeToLiveAsync(sessionKey);
        
            if(ttl == null || ttl < TimeSpan.Zero)
            {
                ttl = TimeSpan.FromSeconds(DEFAULT_LOCK_DURATION_SECONDS);

                await db.HashSetAsync(sessionKey, new HashEntry[]
                {
                    new("status","HOLDING"),
                    new("extended","0"),
                    new("createdAt",DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                });

                var result = await db.KeyExpireAsync(sessionKey, ttl);

                if (!result)
                    return TimeSpan.Zero;

                return ttl.Value;
            }
            return ttl.Value;
        }

        public SeatLockService(IConnectionMultiplexer redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        }

        /// <summary>
        /// Lock multiple seats atomically for a user
        /// Uses Lua script to ensure atomicity
        /// </summary>
        public async Task<bool> LockSeatsAsync(Guid showingId, List<string> seatIds, string userId)
        {
            if (!seatIds.Any()) return true;

            var db = _redis.GetDatabase();

            var sessionKey = GetSessionKey(showingId, userId);

            var ttlSession = await GetOrCreateSessionTtlAsync(db, sessionKey);

            if (ttlSession <= TimeSpan.Zero)
                return false;

            var ttlSeconds = (int)Math.Ceiling(ttlSession.TotalSeconds);

            var seatLockKeys = seatIds.Select(seatId => (RedisKey)$"{_lockPrefix}{showingId}:{seatId}")
                .ToList();

            var zsetKey = (RedisKey)$"{_zsetPrefix}{showingId}";

            // KEYS = seat keys + zset key
            var keys = seatLockKeys.Append(zsetKey).ToArray();


            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var args = new List<RedisValue>
            {
                userId,
                ttlSeconds,
                now
            };

            args.AddRange(seatIds.Select(s => (RedisValue)s));

            var result = (long)await db.ScriptEvaluateAsync(
                LUA_MULTI_LOCK,
                keys,
                args.ToArray());

            if (result == 0) return false;

            var userKey = GetUserKey(showingId, userId);

            await db.SetAddAsync(userKey, seatIds.Select(s => (RedisValue)s).ToArray());

            await db.KeyExpireAsync(userKey, ttlSession);

            return true;
        }

        /// <summary>
        /// Unlock multiple seats atomically
        /// Uses Lua script to ensure only owner can unlock
        /// </summary>
        public async Task<bool> UnlockSeatsAsync(Guid showingId, List<string> seatIds, string userId)
        {
            if (!seatIds.Any()) return true;

            var db = _redis.GetDatabase();

            var seatKeys = seatIds
                .Select(seatId => (RedisKey)$"{_lockPrefix}{showingId}:{seatId}")
                .ToList();

            var userKey = (RedisKey)GetUserKey(showingId, userId);
            var zsetKey = (RedisKey)$"{_zsetPrefix}{showingId}";

            var keys = seatKeys
            .Append(userKey)
            .Append(zsetKey)
            .ToArray();

            var args = new List<RedisValue> { userId };
            args.AddRange(seatIds.Select(s => (RedisValue)s));

            await db.ScriptEvaluateAsync(
                LUA_MULTI_UNLOCK,
                keys,
                args.ToArray());

            var remainingSeats = await db.SetLengthAsync(userKey);
            if (remainingSeats == 0)
            {
                await db.KeyDeleteAsync(userKey);

                var sessionKey = GetSessionKey(showingId, userId);
                await db.KeyDeleteAsync(sessionKey);
            }

            return true;
        }

        public async Task<List<string>> GetUserLockedSeatsAsync(Guid showingId, string userId)
        {
            var db = _redis.GetDatabase();
            var userKey = GetUserKey(showingId, userId);

            var values = await db.SetMembersAsync(userKey);

            return values.Select(x => x.ToString()).ToList();
        }

        /// <summary>
        /// Count locked seats in a showing
        /// </summary>
        public async Task<int> CountLockedSeatsAsync(Guid showingId)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var db = _redis.GetDatabase();

            var zsetKey = (RedisKey)$"{_zsetPrefix}{showingId}";
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await db.SortedSetRemoveRangeByScoreAsync(
                zsetKey,
                double.NegativeInfinity,
                now
            );
            // lấy danh sách seatId đang lock có expire time > now
            var activeSeats = await db.SortedSetLengthAsync(
               zsetKey,
               now,
               double.PositiveInfinity
           );

            return (int) activeSeats;

        }

        public async Task<(List<string> locked, List<string> unlocked, List<string> failed)>
           SyncUserSeatsAsync(Guid showingId, List<string> requestSeats, string userId)
        {
            var current = await GetUserLockedSeatsAsync(showingId, userId);

            var toAdd = requestSeats.Except(current).ToList();
            var toRemove = current.Except(requestSeats).ToList();

            var locked = new List<string>();
            var failed = new List<string>();

            // LOCK trước
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

            // UNLOCK sau
            if (toRemove.Any())
            {
                await UnlockSeatsAsync(showingId, toRemove, userId);
            }

            return (locked, toRemove, failed);
        }

        /// <summary>
        /// Lấy toàn bộ seat đang hold của tất cả user theo showing
        /// Params: showingId
        /// Returns: Danh sách seatId đang bị lock
        /// </summary>
        public async Task<List<Guid>> GetAllLockedSeatsByShowingAsync(Guid showingId)
        {
            var db = _redis.GetDatabase();

            var zsetKey = (RedisKey)$"{_zsetPrefix}{showingId}";
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Xóa các lock đã hết hạn
            await db.SortedSetRemoveRangeByScoreAsync(
                zsetKey,
                double.NegativeInfinity,
                now
            );

            // Lấy danh sách seatId đang lock có expire time > now
            var seats = await db.SortedSetRangeByScoreAsync(
               zsetKey,
               now,
               double.PositiveInfinity);

            if(seats.Length == 0)
                return new List<Guid>();

            return seats.Select(s => Guid.Parse(s!)).ToList();
        }

        /// <summary>
        /// Lấy danh sách user đang hold ghế theo showingId và userId
        /// Params: showingId, userId
        /// Returns: HashSet seatId đang hold của user
        /// </summary>
        public async Task<HashSet<Guid>> GetUserHoldingSeatsAsync(Guid showingId, string userId)
        {
            var db = _redis.GetDatabase();
            var userKey = GetUserKey(showingId, userId);

            var seatValues = await db.SetMembersAsync(userKey);

            if(seatValues.Length == 0)
                return new HashSet<Guid>();

            var zsetKey = (RedisKey)$"{_zsetPrefix}{showingId}";
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var result = new HashSet<Guid>();
            var staleSeats = new List<RedisValue>();

            foreach(var seatValue in seatValues)
            {
                var seatId = seatValue.ToString();
               
                var scoreExprise = await db.SortedSetScoreAsync(zsetKey, seatId);

                if(scoreExprise != null && scoreExprise > now)
                {
                    result.Add(Guid.Parse(seatId!));
                }
                else
                {
                    // Nếu lock đã hết hạn, đánh dấu để xoá khỏi set user
                    staleSeats.Add(seatValue);
                }
            }
            if(staleSeats.Count > 0)
            {
                await db.SetRemoveAsync(userKey, staleSeats.ToArray());
            }

            return result;
        }

        public async Task<bool> MarkSessionAsPayingAsync(Guid showingId, string userId)
        {
            var db = _redis.GetDatabase();

            var sessionKey = GetSessionKey(showingId, userId);
            var userKey = GetUserKey(showingId, userId);
            var zsetKey = $"{_zsetPrefix}{showingId}";

            var key = new RedisKey[]
            {
                sessionKey,
                userKey,
                zsetKey
            };

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var result = (long)await db.ScriptEvaluateAsync(
                LUA_CHANGE_STATUS,
                key,
                new RedisValue[]
                {
                    showingId.ToString(),
                    120,
                    now
                });

            return result == 1;
            
        }

        public async Task<int?> GetSessionRemainingSeconds(Guid showingId, string userId)
        {
            var db = _redis.GetDatabase();
            var sessionKey = GetSessionKey(showingId, userId);

            var ttl = await db.KeyTimeToLiveAsync(sessionKey);

            if (ttl == null || ttl <= TimeSpan.Zero)
                return null;

            return (int)Math.Ceiling(ttl.Value.TotalSeconds);
        }
    }
}
