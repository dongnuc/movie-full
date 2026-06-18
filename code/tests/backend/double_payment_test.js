import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

// ─────────────────────────────────────────────────────────────
// METRICS
// ─────────────────────────────────────────────────────────────
const successCount = new Counter('booking_success_count');

// ─────────────────────────────────────────────────────────────
// CONFIG — điền giá trị thật trước khi chạy
// ─────────────────────────────────────────────────────────────
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5241';
const ENDPOINT = `${BASE_URL}/api/v1/bookings`;

// accessToken phải thuộc 1 user ĐANG có session active + đang
// hold ít nhất 1 ghế hợp lệ cho showingId dưới đây
const ACCESS_TOKEN = __ENV.ACCESS_TOKEN || 'REPLACE_WITH_REAL_ACCESS_TOKEN';
const SHOWING_ID   = __ENV.SHOWING_ID   || 'REPLACE_WITH_REAL_SHOWING_ID';

// Độ trễ giữa request 1 và request 2 (ms).
//   0   → mô phỏng race condition tệ nhất (network auto-retry,
//         2 request bắn đồng thời tuyệt đối)
//   50-150 → mô phỏng double-click tay run thực tế
const CLICK_DELAY_MS = Number(__ENV.CLICK_DELAY_MS || 0);

export const options = {
    scenarios: {
        double_payment_race: {
            executor: 'shared-iterations',
            vus: 2,           // đúng 2 virtual user, cùng accessToken
            iterations: 2,    // đúng 2 request, mỗi VU gọi 1 lần
            maxDuration: '15s',
        },
    },
};

export default function () {
    // VU 2 sleep một chút để mô phỏng double-click cách nhau vài chục ms
    // (đặt CLICK_DELAY_MS=0 để test trường hợp đồng thời tuyệt đối)
    if (CLICK_DELAY_MS > 0 && __VU === 2) {
        sleep(CLICK_DELAY_MS / 1000);
    }

    const payload = JSON.stringify({
        showingId: SHOWING_ID,
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${ACCESS_TOKEN}`,
        },
        timeout: '15s',
    };

    const start = Date.now();
    const res = http.post(ENDPOINT, payload, params);
    const durationMs = Date.now() - start;

    const isSuccess = res.status === 200 || res.status === 201;

    check(res, {
        'status is 2xx or properly rejected (400/409)': (r) =>
            [200, 201, 400, 409].includes(r.status),
    });

    if (isSuccess) {
        successCount.add(1);
    }

    console.log(
        `[VU ${__VU}] status=${res.status} duration=${durationMs}ms body=${res.body}`
    );
}

// ─────────────────────────────────────────────────────────────
// Tổng kết sau khi chạy — đọc trực tiếp console, không cần file
// ─────────────────────────────────────────────────────────────
export function handleSummary(data) {
    const successes = data.metrics.booking_success_count
        ? data.metrics.booking_success_count.values.count
        : 0;

    let verdict;
    if (successes >= 2) {
        verdict = '🔴 BUG CONFIRMED: Cả 2 request đều tạo booking thành công — kiểm tra DB xem có 2 bookingId khác nhau cùng trừ điểm không.';
    } else if (successes === 1) {
        verdict = '🟢 OK: Chỉ 1 request thành công, request thứ 2 bị chặn đúng.';
    } else {
        verdict = '🟡 KHÔNG XÁC ĐỊNH: Không request nào thành công — kiểm tra lại accessToken/showingId/session ghế còn hold hợp lệ không.';
    }

    console.log('\n========================================');
    console.log(`Số request thành công: ${successes} / 2`);
    console.log(verdict);
    console.log('========================================\n');

    return { stdout: '' }; // không in lại toàn bộ JSON summary, đỡ rối console
}