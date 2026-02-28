import http from 'k6/http';
import { check, sleep } from 'k6';
import {
  buildHeaders,
  buildOptions,
  buildUrl,
  getResponseData,
  getKeyword,
  getSleep,
  hasPath,
  isJsonResponse,
  logFailure,
  parseJson,
  recordLatency,
} from './helpers.js';

export const options = buildOptions('signature-control');

export default function () {
  const keyword = getKeyword();
  const url = buildUrl(`/document-services-s/v1/signature-request/control?keyword=${keyword}`);

  const response = http.put(url, null, {
    headers: buildHeaders(),
  });

  recordLatency(response);

  const json = parseJson(response);
  const data = getResponseData(json);

  const ok = check(response, {
    'status is 200': (res) => res.status === 200,
    'response is json': (res) => isJsonResponse(res),
    'response has data': () => data !== null,
    'response has message': () => hasPath(data, 'message'),
    'response has estado': () => hasPath(data, 'estado'),
  });

  if (!ok) {
    logFailure(response, 'signature-control');
  }

  sleep(getSleep());
}
