import http from 'k6/http';
import { check, group, sleep } from 'k6';

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

const API_BASE = __ENV.API_BASE || 'http://localhost:5035';
const LLM_BASE = __ENV.LLM_BASE || 'http://localhost:8001/v1/chat/completions';

function postJson(url, body) {
  return http.post(url, JSON.stringify(body), {
    headers: { 'Content-Type': 'application/json' },
  });
}

export default function () {
  group('Interview Full-Flow with Mocked LLM', () => {
    // 1) init
    let init = postJson(`${API_BASE}/api/interview/init`, {
      email: `loadtest+${__VU}@example.com`,
      jobDescription: 'Load test run',
    });
    check(init, { 'init 200': r => r.status === 200 });
    let sessionId = init.json('sessionId');

    // 2) questions loop
    let question;
    do {
      let q = http.get(`${API_BASE}/api/interview/question?sessionId=${sessionId}`);
      check(q, { 'q status 200': r => r.status === 200 });
      question = q.json('question');
      if (!question) break;

      // 3) hit mock LLM
      let llmResp = postJson(LLM_BASE, {
        model: 'gpt-4',
        messages: [{ role: 'user', content: question }],
      });
      check(llmResp, { 'llm mock 200': r => r.status === 200 });

      // 4) send answer
      let ans = postJson(`${API_BASE}/api/interview/answer`, {
        sessionId,
        question,
        transcript: llmResp.json('choices[0].message.content'),
        audioBase64: 'A'.repeat(6000),
      });
      check(ans, { 'ans 200': r => r.status === 200 });

      sleep(1);
    } while (question);

    // 5) complete
    check(
      http.post(`${API_BASE}/api/interview/complete?sessionId=${sessionId}`),
      { 'complete 200': r => r.status === 200 }
    );
  });

  sleep(2);
}
