/**
 * generate_tokens.js
 * ─────────────────────────────────────────────────────────────
 * Generate 1000 JWT access token hợp lệ để test load (k6),
 * KHÔNG cần gọi API /auth/login thật cho từng user.
 *
 * Yêu cầu: SecretKey ký JWT phải GIỐNG với SecretKey backend
 * đang dùng để verify token (lấy từ appsettings.json /
 * appsettings.Development.json, mục Jwt:Secret hoặc tương tự).
 *
 * Đồng thời script này sẽ nạp thông tin token vào Redis cache
 * để vượt qua cơ chế kiểm tra token_revoked trong TokenValidationMiddleware.
 *
 * Cách dùng:
 *   1. Sửa CONFIG dưới đây cho khớp appsettings của bạn
 *   2. node generate_tokens.js
 *   3. File tokens.json sẽ được tạo ra, dùng trong k6 script
 * ─────────────────────────────────────────────────────────────
 */

const jwt = require('jsonwebtoken');
const fs = require('fs');
const crypto = require('crypto');
const Redis = require('ioredis');

// ─────────────────────────────────────────────────────────────
// CONFIG — PHẢI khớp với appsettings.json của backend
// ─────────────────────────────────────────────────────────────
const CONFIG = {
    secret: process.env.JWT_SECRET || 'xT9@vP4#Lm2Qz8K!aD6rY0WcF3uH7sN1bE5Jg$UqRkMZpXnCwVt8yA2hB',
    audience: 'MovieAL',
    issuer: 'MovieOwner',
    expiresInSeconds: 60 * 60 * 2, // 2 giờ, đủ thời gian chạy test
    redis: {
        host: process.env.REDIS_HOST || '127.0.0.1',
        port: Number(process.env.REDIS_PORT || 6379),
        prefix: process.env.REDIS_PREFIX || 'MovieApp:',
        db: Number(process.env.REDIS_DB || 0),
    }
};

const NUMBER_OF_USERS = Number(process.env.NUMBER_OF_USERS || 1000);

async function main() {
    // ─────────────────────────────────────────────────────────────
    // Đọc danh sách userId thật từ DB đã export ra CSV/JSON
    // Format file users_seed.json (tự tạo, xem hướng dẫn dưới):
    //   [{ "userId": "guid-thật-1" }, { "userId": "guid-thật-2" }, ...]
    // ─────────────────────────────────────────────────────────────
    let userSeeds = [];
    const seedFile = 'users_seed.json';

    if (fs.existsSync(seedFile)) {
        userSeeds = JSON.parse(fs.readFileSync(seedFile, 'utf8'));
        console.log(`Đã đọc ${userSeeds.length} user thật từ ${seedFile}`);
    } else {
        console.warn(
            `⚠ Không tìm thấy ${seedFile}. Sẽ generate userId NGẪU NHIÊN ` +
            `(KHÔNG tồn tại trong DB thật). Điều này CHỈ phù hợp nếu ` +
            `backend không kiểm tra userId có tồn tại trong DB khi xử lý request — ` +
            `nếu backend có query User table theo Sub claim, BẮT BUỘC phải dùng userId thật.`
        );
        for (let i = 0; i < NUMBER_OF_USERS; i++) {
            userSeeds.push({ userId: crypto.randomUUID() });
        }
    }

    if (userSeeds.length < NUMBER_OF_USERS) {
        console.warn(
            `⚠ Chỉ có ${userSeeds.length} user thật nhưng cần ${NUMBER_OF_USERS}. ` +
            `Sẽ lặp lại danh sách user để đủ số lượng.`
        );
    }

    // Kết nối Redis
    console.log(`Đang kết nối tới Redis tại ${CONFIG.redis.host}:${CONFIG.redis.port} (db: ${CONFIG.redis.db})...`);
    const redis = new Redis({
        host: CONFIG.redis.host,
        port: CONFIG.redis.port,
        db: CONFIG.redis.db,
    });

    const pipeline = redis.pipeline();
    const tokens = [];
    const now = Math.floor(Date.now() / 1000);

    console.log(`Đang sinh ${NUMBER_OF_USERS} token và chuẩn bị nạp vào Redis...`);

    for (let i = 0; i < NUMBER_OF_USERS; i++) {
        const seed = userSeeds[i % userSeeds.length];
        const userId = seed.userId;
        const accessJti = crypto.randomUUID();
        const sessionJti = crypto.randomUUID(); // đóng vai trò refresh token JTI / sessionId

        const payload = {
            aud: CONFIG.audience,
            iss: CONFIG.issuer,
            sub: userId,
            name: `loadtest_user_${i}`,
            email: `loadtest_user_${i}@test.com`,
            jti: accessJti,
            token_type: 'access',
            role: seed.role || 'USER',          // điều chỉnh role nếu cần
            iat: now,
            nbf: now,
            exp: now + CONFIG.expiresInSeconds,
        };

        // Nếu role của bạn cần theaterId, bỏ comment dòng dưới
        // payload.theaterId = seed.theaterId || null;

        const token = jwt.sign(payload, CONFIG.secret, { algorithm: 'HS256' });

        tokens.push({
            userId,
            token,
        });

        // Đẩy lệnh ghi vào Redis Pipeline
        const redisKey = `${CONFIG.redis.prefix}auth:access:${accessJti}`;
        pipeline.hset(redisKey, 'userId', userId, 'sessionId', sessionJti);
        pipeline.expire(redisKey, CONFIG.expiresInSeconds);
    }

    // Thực thi nạp dữ liệu vào Redis hàng loạt (Bulk/Pipeline) để tối ưu hiệu năng
    try {
        console.log(`Đang nạp các keys vào Redis...`);
        const results = await pipeline.exec();
        
        // Kiểm tra xem có lệnh nào bị lỗi trong pipeline không
        const errors = results.filter(([err]) => err);
        if (errors.length > 0) {
            console.error(`⚠ Có ${errors.length} lỗi khi nạp key vào Redis.`);
        } else {
            console.log(`✓ Đã nạp thành công các keys Token vào Redis.`);
        }
    } catch (redisError) {
        console.error('❌ Lỗi kết nối hoặc ghi vào Redis:', redisError.message);
        console.warn('⚠ Các token vẫn được lưu ra file nhưng sẽ không hoạt động nếu backend bật kiểm tra cache.');
    } finally {
        redis.disconnect();
    }

    fs.writeFileSync('tokens.json', JSON.stringify(tokens, null, 2));
    console.log(`✓ Đã tạo ${tokens.length} token, lưu vào tokens.json`);
}

main().catch(err => {
    console.error('❌ Lỗi thực thi:', err);
});