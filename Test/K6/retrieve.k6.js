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

export const options = buildOptions('signature-retrieve');

export default function () {
  const keyword = getKeyword();
  const url = buildUrl(`/document-services-s/v1/signature-request/retrieve?keyword=${keyword}`);

  const response = http.get(url, {
    headers: buildHeaders(),
  });

  recordLatency(response);

  const json = parseJson(response);
  const data = getResponseData(json);

  const ok = check(response, {
    'status is 200': (res) => res.status === 200,
    'response is json': (res) => isJsonResponse(res),
    'response has data': () => data !== null,
    'response has keyword': () => hasPath(data, 'keyword'),
    'response has idOrdenKeynua': () => hasPath(data, 'idOrdenKeynua'),
    'response has cEstadoGeneral': () => hasPath(data, 'cEstadoGeneral'),
    'response has arrFirmantes': () => Array.isArray(data?.arrFirmantes),
  });

  if (!ok) {
    logFailure(response, 'signature-retrieve');
  }

  sleep(getSleep());
}
