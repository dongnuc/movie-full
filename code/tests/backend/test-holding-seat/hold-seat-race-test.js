import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';
import { SharedArray } from 'k6/data';

// ─────────────────────────────────────────────────────────────
// METRICS — đo lường để xác nhận hold ghế chỉ thành công 1 lần
// ─────────────────────────────────────────────────────────────
const successCount = new Counter('hold_success_count');
const conflictCount = new Counter('hold_conflict_count');     // 400/409 — chặn đúng cách
const crashCount = new Counter('hold_crash_count');           // 5xx — cần fix

// ─────────────────────────────────────────────────────────────
// CONFIG
// ─────────────────────────────────────────────────────────────
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5241';
const ENDPOINT = `${BASE_URL}/api/v1/showing-seats/hold`;

// Ghế DUY NHẤT mà tất cả 1000 user sẽ cùng cố gắng hold
const SHOWING_ID = __ENV.SHOWING_ID || 'f41b5932-7a04-478f-a313-0b6c6fbfd05c';
const SEAT_ID    = __ENV.SEAT_ID    || 'C03CB339-AEB9-4094-A137-1A0E867A7D21';

// Đọc danh sách token đã generate từ generate_tokens.js
// File tokens.json nằm cùng thư mục với script này
const tokens = new SharedArray('tokens', function () {
    return JSON.parse(open('./tokens.json'));
});

export const options = {
    scenarios: {
        hold_seat_race: {
            executor: 'shared-iterations',
            vus: Math.min(tokens.length, 1000),  // tối đa số token có sẵn
            iterations: tokens.length,            // mỗi token gọi đúng 1 lần
            maxDuration: '60s',
        },
    },
};

export default function () {
    // Mỗi VU dùng 1 token riêng — đảm bảo mỗi request là 1 "user" khác nhau
    const tokenEntry = tokens[(__VU - 1) % tokens.length];

    const payload = JSON.stringify({
        showingId: SHOWING_ID,
        seatIds: [SEAT_ID],
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${tokenEntry.token}`,
        },
        timeout: '15s',
    };

    const res = http.post(ENDPOINT, payload, params);

    const isSuccess = res.status === 200 || res.status === 201;
    const isConflict = res.status === 400 || res.status === 409;
    const isCrash = res.status >= 500;

    check(res, {
        'status is not 5xx': (r) => r.status < 500,
    });

    if (isSuccess) {
        successCount.add(1);
    } else if (isConflict) {
        conflictCount.add(1);
    } else if (isCrash) {
        crashCount.add(1);
    }

    // Chỉ log khi cần debug — với 1000 request, log toàn bộ sẽ rất nhiều
    if (__ENV.VERBOSE === 'true') {
        console.log(`[VU ${__VU}] status=${res.status} body=${res.body}`);
    }
}

export function handleSummary(data) {
    const successes = data.metrics.hold_success_count
        ? data.metrics.hold_success_count.values.count : 0;
    const conflicts = data.metrics.hold_conflict_count
        ? data.metrics.hold_conflict_count.values.count : 0;
    const crashes = data.metrics.hold_crash_count
        ? data.metrics.hold_crash_count.values.count : 0;

    let verdict;
    if (successes === 1 && crashes === 0) {
        verdict = '🟢 ĐÚNG: Chỉ đúng 1 user hold thành công, còn lại bị chặn đúng cách (400/409).';
    } else if (successes === 1 && crashes > 0) {
        verdict = `🟠 CẦN XEM LẠI: 1 user hold thành công, nhưng có ${crashes} request bị crash (5xx) ` +
                  'thay vì bị chặn đúng cách. Dữ liệu có thể vẫn an toàn nhưng error handling chưa tốt.';
    } else if (successes === 0) {
        verdict = '🟡 KHÔNG XÁC ĐỊNH: Không ai hold thành công — kiểm tra lại SHOWING_ID/SEAT_ID có hợp lệ, ' +
                   'ghế có đang ở trạng thái Available trước khi test không.';
    } else {
        verdict = `🔴 BUG NGHIÊM TRỌNG: Có ${successes} user CÙNG hold thành công 1 ghế — ` +
                   'race condition KHÔNG được xử lý đúng. Kiểm tra DB ngay (Status/HoldBy của ghế này).';
    }

    console.log('\n========================================');
    console.log(`Tổng request: ${data.metrics.iterations ? data.metrics.iterations.values.count : 'N/A'}`);
    console.log(`Thành công: ${successes} | Bị chặn đúng (4xx): ${conflicts} | Crash (5xx): ${crashes}`);
    console.log(verdict);
    console.log('========================================\n');

    return { stdout: '' };
}