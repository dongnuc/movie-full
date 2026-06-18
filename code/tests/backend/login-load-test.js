// login-load-test.js
import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';

const successCounter = new Counter('login_success');
const failCounter = new Counter('login_fail');

const BASE_URL = 'http://localhost:5241';
const PASSWORD = 'String123@';

export const options = {
  scenarios: {
    login_spike: {
      executor: 'per-vu-iterations',
      vus: 500,
      iterations: 1,
      maxDuration: '30s',
    },
  },
  insecureSkipTLSVerify: true,
};

export default function () {
  const userIndex = __VU;
  const username = `loadtest_user_${userIndex}`;

  const res = http.post(
    `${BASE_URL}/api/v1/auth/login`,
    JSON.stringify({ usernameOrEmail: username, password: PASSWORD }),
    { headers: { 'Content-Type': 'application/json' } }
  );

  let body = {};
  try {
    body = JSON.parse(res.body);
  } catch {
    body = {};
  }

  const ok = check(res, {
    'status is 200': (r) => r.status === 200,
    'has accessToken': () => body.accessToken !== undefined,
    'has accessTokenExpiresAt': () => body.accessTokenExpiresAt !== undefined,
    'has refresh cookie set': (r) => r.cookies && Object.keys(r.cookies).length > 0,
  });

  if (ok) {
    successCounter.add(1);
  } else {
    failCounter.add(1);
    console.error(`Login failed for ${username}: ${res.status} - ${res.body}`);
  }
}