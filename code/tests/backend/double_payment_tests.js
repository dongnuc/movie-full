import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

// ─────────────────────────────────────────────────────────────
// TEST CASES — mỗi cặp: 1 user đang hold ghế hợp lệ
// ─────────────────────────────────────────────────────────────
const TEST_CASES = [
    {
        label:     'User 1',
        token:     'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJNb3ZpZUFMIiwiaXNzIjoiTW92aWVPd25lciIsImV4cCI6MTc4MTc3MTk4MSwic3ViIjoiYzgwMjM1M2MtZDdlZS00NGRkLTkzYzQtMDAwMTQ0ZTAzYTRmIiwibmFtZSI6ImxvYWR0ZXN0X3VzZXJfMjgiLCJlbWFpbCI6ImxvYWR0ZXN0X3VzZXJfMjhAdGVzdC5jb20iLCJqdGkiOiJlYTIwNjFiMC1kZmNjLTRmYzktYWI5Yi00MzBmNmE3OTY4ZTAiLCJ0b2tlbl90eXBlIjoiYWNjZXNzIiwiaWF0IjoxNzgxNzcwMTgxLCJuYmYiOjE3ODE3NzAxODF9.M4r93VIWNQ_LLYou6EFX6v5QHPyyrxVG4pWuezYJdgM',
        showingId: '67574c89-d14d-4b90-82e5-a3dfc420c6c6',
    },
    {
        label:     'User 2',
        token:     'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJNb3ZpZUFMIiwiaXNzIjoiTW92aWVPd25lciIsImV4cCI6MTc4MTc3MjAxOSwic3ViIjoiYTdjNWJlN2ItMDI2NC00OTMzLWI4MDktZjg0NDg5OGE1ZGVjIiwibmFtZSI6ImxvYWR0ZXN0X3VzZXJfMjkiLCJlbWFpbCI6ImxvYWR0ZXN0X3VzZXJfMjlAdGVzdC5jb20iLCJqdGkiOiI5ODY2ODNiZS1hMWQ3LTQ3NGQtODc4OC04NWViMmE0YWNjN2MiLCJ0b2tlbl90eXBlIjoiYWNjZXNzIiwiaWF0IjoxNzgxNzcwMjE5LCJuYmYiOjE3ODE3NzAyMTl9.9fMkUgrdv5mmWVhge_97McuDJ8ypbe95juWXKcEgn0E',
        showingId: '67574c89-d14d-4b90-82e5-a3dfc420c6c6',
    },
    {
        label:     'User 3',
        token:     'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJNb3ZpZUFMIiwiaXNzIjoiTW92aWVPd25lciIsImV4cCI6MTc4MTc3MjAzMSwic3ViIjoiODQzZWY0NmEtNmQ5Yi00OWFiLWFhOTMtYzBiYzRmN2ZjMjU2IiwibmFtZSI6ImxvYWR0ZXN0X3VzZXJfMzAiLCJlbWFpbCI6ImxvYWR0ZXN0X3VzZXJfMzBAdGVzdC5jb20iLCJqdGkiOiI2NTI4YTBkNy1iZjUwLTRkZmYtYTI3Mi1kNzczNzJjOGRjYWUiLCJ0b2tlbl90eXBlIjoiYWNjZXNzIiwiaWF0IjoxNzgxNzcwMjMxLCJuYmYiOjE3ODE3NzAyMzF9.g1l536qAoOEHBcPoRLXHvoy5GR3N784nnoe_B49Teic',
        showingId: '67574c89-d14d-4b90-82e5-a3dfc420c6c6',
    },
    {
        label:     'User 4',
        token:     'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJNb3ZpZUFMIiwiaXNzIjoiTW92aWVPd25lciIsImV4cCI6MTc4MTc3MjA2MSwic3ViIjoiMTgxYTMxYzItZjFkNi00ZGViLTgyYjItYWZkYWQwMzNhNGQwIiwibmFtZSI6ImxvYWR0ZXN0X3VzZXJfMzIiLCJlbWFpbCI6ImxvYWR0ZXN0X3VzZXJfMzJAdGVzdC5jb20iLCJqdGkiOiJkOTA0ZjUzZS03NzA5LTQyYTQtYTY2MC1hMjlmNTYzM2I0YjIiLCJ0b2tlbl90eXBlIjoiYWNjZXNzIiwiaWF0IjoxNzgxNzcwMjYxLCJuYmYiOjE3ODE3NzAyNjF9.uQam4ILWCVoklTckhOWiRWi6KjHI-D4sR8ACcOZJlk8',
        showingId: '67574c89-d14d-4b90-82e5-a3dfc420c6c6',
    },
];

// ─────────────────────────────────────────────────────────────
// CONFIG
// ─────────────────────────────────────────────────────────────
const BASE_URL       = __ENV.BASE_URL || 'http://localhost:5241';
const ENDPOINT       = `${BASE_URL}/api/v1/bookings`;
const CLICK_DELAY_MS = Number(__ENV.CLICK_DELAY_MS || 50);

const NUM_USERS = TEST_CASES.length;
const TOTAL_VUS = NUM_USERS * 2;

// ─────────────────────────────────────────────────────────────
// METRICS
// ─────────────────────────────────────────────────────────────
const successCount = new Counter('booking_success_count');

export const options = {
    scenarios: {
        double_payment_test: {
            executor:    'shared-iterations',
            vus:         TOTAL_VUS,
            iterations:  TOTAL_VUS,
            maxDuration: '30s',
        },
    },
    thresholds: {
        'booking_success_count': [`count<=${NUM_USERS}`],
    },
};

export default function () {
    const tc            = TEST_CASES[Math.floor((__VU - 1) / 2)];
    const isSecondClick = __VU % 2 === 0;

    if (isSecondClick) sleep(CLICK_DELAY_MS / 1000);

    const res = http.post(
        ENDPOINT,
        JSON.stringify({ showingId: tc.showingId }),
        {
            headers: {
                'Content-Type':  'application/json',
                'Authorization': `Bearer ${tc.token}`,
            },
            timeout: '15s',
        }
    );

    const isSuccess = res.status === 200 || res.status === 201;

    check(res, {
        'status hợp lệ (2xx / 400 / 409)':
            (r) => [200, 201, 400, 409].includes(r.status),
    });

    if (isSuccess) successCount.add(1);

    console.log(
        `[${tc.label} | click ${isSecondClick ? 2 : 1}]` +
        ` status=${res.status} body=${res.body}`
    );
}

// ─────────────────────────────────────────────────────────────
// SUMMARY
// ─────────────────────────────────────────────────────────────
export function handleSummary(data) {
    const successes = data.metrics.booking_success_count?.values?.count ?? 0;

    const lines = [
        '',
        '══════════════════════════════════════════════',
        ` Số user test       : ${NUM_USERS}`,
        ` Request thành công : ${successes} / ${TOTAL_VUS}`,
        ` Kỳ vọng            : tối đa ${NUM_USERS} (mỗi user 1 lần trừ tiền)`,
        '──────────────────────────────────────────────',
    ];

    if (successes > NUM_USERS) {
        lines.push(` 🔴 BUG: ${successes - NUM_USERS} user bị trừ tiền 2 lần`);
        lines.push(`    → Kiểm tra idempotency key / DB unique constraint / distributed lock`);
    } else if (successes === NUM_USERS) {
        lines.push(` 🟢 OK: Không user nào bị trừ tiền 2 lần`);
    } else {
        lines.push(` 🟡 PARTIAL (${successes}/${NUM_USERS}) — xem log từng VU, có thể token/session hết hạn`);
    }

    lines.push('══════════════════════════════════════════════', '');
    console.log(lines.join('\n'));

    return { stdout: '' };
}