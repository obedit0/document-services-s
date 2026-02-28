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
import { buildUpdateDocumentsPayload } from './payloads.js';

export const options = buildOptions('signature-documents-persisted');

export default function () {
  const url = buildUrl('/document-services-s/v1/signature-request/documents-persisted');
  const keyword = getKeyword();
  const payload = buildUpdateDocumentsPayload(keyword);

  const response = http.put(url, JSON.stringify(payload), {
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
    'response has documentosActualizados': () => hasPath(data, 'documentosActualizados'),
  });

  if (!ok) {
    logFailure(response, 'signature-documents-persisted');
  }

  sleep(getSleep());
}
