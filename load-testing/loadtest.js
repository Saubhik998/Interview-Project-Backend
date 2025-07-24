import http from 'k6/http';
import { check, group, sleep } from 'k6';

/**
 * k6 execution options defining virtual user ramping and performance thresholds.
 * 
 * - Stages:
 *   * 1m ramp to 100 VUs
 *   * 2m stay at 100 VUs
 *   * 1m ramp down to 0 VUs
 * - Thresholds:
 *   * http_req_duration: 95% must be below 5s
 *   * http_req_failed: Less than 1% failed requests
 * @type {import('k6/options').Options}
 */
export const options = {
  stages: [
    { duration: '1m', target: 100 },
    { duration: '2m', target: 100 },
    { duration: '1m', target:   0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<5000'],
    http_req_failed:   ['rate<0.01'],
  },
};

/**
 * API base URLs, overridable via environment variables.
 * @type {string}
 */
const API_BASE = __ENV.API_BASE || 'http://localhost:5035';
const LLM_BASE = __ENV.LLM_BASE || 'http://localhost:8001/v1/chat/completions';

/**
 * Helper to POST JSON to any URL with application/json header.
 * @param {string} url - Target URL.
 * @param {object} body - JSON body object.
 * @returns {import('k6/http').Response}
 */
function postJson(url, body) {
  return http.post(url, JSON.stringify(body), {
    headers: { 'Content-Type': 'application/json' },
  });
}

/**
 * Performs the full interview flow:
 *  - Initializes the interview
 *  - Loops: Fetch next question => Gets answer from mock LLM => Submits answer
 *  - Completes the interview
 * Each user (VU) uses a unique email address.
 */
export default function () {
  group('Interview Full-Flow with Mocked LLM', () => {
    // 1. Initialize Interview Session
    /** @type {import('k6/http').Response} */
    let init = postJson(`${API_BASE}/api/interview/init`, {
      email: `loadtest+${__VU}@example.com`, // unique per VU
      jobDescription: 'Load test run',
    });
    check(init, { 'init 200': r => r.status === 200 });

    /** @type {string} */
    let sessionId = init.json('sessionId');

    // 2. Interview Q/A Loop
    let question;
    do {
      // Get the next question in the interview
      let q = http.get(`${API_BASE}/api/interview/question?sessionId=${sessionId}`);
      check(q, { 'q status 200': r => r.status === 200 });
      question = q.json('question');
      if (!question) break;

      // 3. Generate answer using mocked LLM API
      let llmResp = postJson(LLM_BASE, {
        model: 'gpt-4',
        messages: [{ role: 'user', content: question }],
      });
      check(llmResp, { 'llm mock 200': r => r.status === 200 });

      // 4. Submit the answer to the API
      let ans = postJson(`${API_BASE}/api/interview/answer`, {
        sessionId,
        question,
        transcript: llmResp.json('choices[0].message.content'),
        audioBase64: 'A'.repeat(6000),  // Simulate ~6KB of base64 audio
      });
      check(ans, { 'ans 200': r => r.status === 200 });

      sleep(1); // Simulate user think time
    } while (question);

    // 5. Complete the interview
    check(
      http.post(`${API_BASE}/api/interview/complete?sessionId=${sessionId}`),
      { 'complete 200': r => r.status === 200 }
    );
  });

  sleep(2); // Cool down between iterations/users
}
