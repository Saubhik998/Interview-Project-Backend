import http from 'k6/http';
import { sleep, check } from 'k6';

export let options = {
  vus: 10,
  duration: '1m',
};

export default function () {
  const baseUrl = 'http://localhost:5035/api/interview';
  const email = `user${Math.floor(Math.random() * 10000)}@test.com`;

  const initPayload = JSON.stringify({
    email,
    jobDescription: "Load test JD for K6",
  });

  let initRes = http.post(`${baseUrl}/init`, initPayload, {
    headers: { 'Content-Type': 'application/json' },
  });

  check(initRes, { 'init: status 200': (r) => r.status === 200 });

  let currentIndex = 0;
  let currentQuestion = initRes.json()?.firstQuestion || "Default fallback question";

  while (currentIndex < 5) {
    const answerPayload = JSON.stringify({
      question: currentQuestion,
      transcript: "This is a dummy answer",
      audioBase64: btoa("dummy audio"),
      index: currentIndex,
    });

    let aRes = http.post(`${baseUrl}/answer`, answerPayload, {
      headers: { 'Content-Type': 'application/json' },
    });

    check(aRes, { 'answer: status 200': (r) => r.status === 200 });

    let qRes = http.get(`${baseUrl}/question`);
    check(qRes, { 'question: status 200': (r) => r.status === 200 });

    let qBody = qRes.json();
    if (qBody?.message === "Interview complete") break;

    currentQuestion = qBody?.question || "Fallback question";
    currentIndex = qBody?.index ?? (currentIndex + 1);

    sleep(0.5);
  }

  let completeRes = http.post(`${baseUrl}/complete`, null, {
    headers: { 'Content-Type': 'application/json' },
  });
  check(completeRes, { 'complete: status 200': (r) => r.status === 200 });

  let reportRes = http.get(`${baseUrl}/report`);
  check(reportRes, { 'report: status 200': (r) => r.status === 200 });

  let reportsRes = http.get(`${baseUrl}/reports?email=${email}`);
  check(reportsRes, { 'reports by email: status 200': (r) => r.status === 200 });

  let reports = reportsRes.json();
  if (Array.isArray(reports) && reports.length > 0) {
    let reportId = reports[0]?.id || reports[0]?._id || reports[0];
    let singleReportRes = http.get(`${baseUrl}/report/${reportId}`);
    check(singleReportRes, { 'report by id: status 200': (r) => r.status === 200 });
  }

  sleep(1);
}

// base64 encoding helper (only needed for dummy audio)
function btoa(str) {
  return Buffer.from(str).toString('base64');
}
