import http from 'k6/http';
import { check, sleep } from 'k6';
import {
  buildHeaders,
  buildOptions,
  buildUrl,
  getResponseData,
  getSleep,
  getUniqueKeyword,
  hasPath,
  isJsonResponse,
  logFailure,
  parseJson,
  recordLatency,
} from './helpers.js';
import { buildInitiatePayload } from './payloads.js';

export const options = buildOptions('signature-initiate');

export default function () {
  const url = buildUrl('/document-services-s/v1/signature-request/initiate');
  const keyword = getUniqueKeyword();
  const payload = buildInitiatePayload(keyword);

  const response = http.post(url, JSON.stringify(payload), {
    headers: buildHeaders(),
  });

  recordLatency(response);

  const json = parseJson(response);
  const data = getResponseData(json);

  const ok = check(response, {
    'status is 200': (res) => res.status === 200,
    'response is json': (res) => isJsonResponse(res),
    'response has data': () => data !== null,
    'response has idFirma': () => hasPath(data, 'idFirma'),
    'response has estado': () => hasPath(data, 'estado'),
    'response has referencia': () => hasPath(data, 'referencia'),
  });

  if (!ok) {
    logFailure(response, 'signature-initiate');
  }

  sleep(getSleep());
}
