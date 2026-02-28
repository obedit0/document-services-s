import { check } from 'k6';
import { Counter, Rate } from 'k6/metrics';
import exec from 'k6/execution';


const DEFAULT_BASE_URL = 'http://localhost:5207';
const DEFAULT_CHANNEL_IDENTIFICATION = '7';
const DEFAULT_KEYWORD = 1000;
const DEFAULT_LOG_BODY_LIMIT = 500;
const DEFAULT_WARMUP_RPS = 5;
const DEFAULT_RPS_TARGETS = [40,50,30]; //para initiate configurar [40,50,30]
const DEFAULT_RPS_RAMP_DURATION = '30s';
const DEFAULT_RPS_HOLD_DURATION = '1m';

export const BASE_URL = normalizeBaseUrl(__ENV.BASE_URL || DEFAULT_BASE_URL);
export const TOKEN = __ENV.TOKEN || '';
export const CHANNEL_IDENTIFICATION =
  __ENV.CHANNEL_IDENTIFICATION || __ENV.CHANNEL_ID || DEFAULT_CHANNEL_IDENTIFICATION;
export const KEYWORD = parseKeyword(__ENV.KEYWORD, DEFAULT_KEYWORD);

export const slowRequestCount = new Counter('slow_request_count');
export const slowRequestRate = new Rate('slow_request_rate');

export function buildUrl(path) {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${BASE_URL}${normalizedPath}`;
}

export function buildHeaders() {
  const headers = {
    'Content-Type': 'application/json',
    messageIdentification: buildMessageId(),
  };
  headers.channelIdentification = CHANNEL_IDENTIFICATION;
  if (TOKEN) {
    headers.Authorization = `Bearer ${TOKEN}`;
  }

  return headers;
}

export function buildMessageId() {
  const raw = `msg-${Date.now()}-${__VU}-${__ITER}`;
  return raw.length > 42 ? raw.slice(0, 42) : raw;
}

export function buildUniqueSuffix() {
  const inc = exec.scenario.iterationInTest + 1;

  return `${inc}`;
}

export function futureIsoDate(daysAhead) {
  const now = Date.now();
  const future = new Date(now + daysAhead * 60 * 60 * 1000);
  return future.toISOString();
}

export function getKeyword() {
  return KEYWORD;
}

export function getUniqueKeyword() {
  const inc = exec.scenario.iterationInTest + 1;
  return KEYWORD + inc;
}

export function getScenarioName() {
  return (__ENV.SCENARIO || 'rps').toLowerCase();
}

export function getSleep() {
  if (__ENV.SLEEP) {
    const parsed = parseFloat(__ENV.SLEEP);
    return Number.isFinite(parsed) ? parsed : 0;
  }

  const scenario = getScenarioName();
  return scenario === 'rps' ? 0 : 1;
}

export function recordLatency(response) {
  const duration = response && response.timings ? response.timings.duration : 0;
  const isSlow = duration > 3000;

  if (isSlow) {
    slowRequestCount.add(1);
  }

  slowRequestRate.add(isSlow);
  return isSlow;
}

export function parseJson(response) {
  try {
    return response.json();
  } catch (error) {
    return null;
  }
}

export function getResponseData(payload) {
  if (!payload || typeof payload !== 'object') {
    return null;
  }

  if (Object.prototype.hasOwnProperty.call(payload, 'response')) {
    return payload.response;
  }

  if (Object.prototype.hasOwnProperty.call(payload, 'data')) {
    return payload.data;
  }

  return null;
}

export function isJsonResponse(response) {
  const contentType = response.headers['Content-Type'] || response.headers['content-type'] || '';
  return contentType.toLowerCase().includes('application/json');
}

export function hasPath(target, path) {
  if (!target || typeof target !== 'object') {
    return false;
  }

  const parts = path.split('.');
  let current = target;

  for (const part of parts) {
    if (!Object.prototype.hasOwnProperty.call(current, part)) {
      return false;
    }

    current = current[part];
    if (current === null || current === undefined) {
      return false;
    }
  }

  return true;
}

export function checkStatus(response, allowedStatuses, label) {
  const name = label || `status is ${allowedStatuses.join(' or ')}`;
  return check(response, {
    [name]: (res) => allowedStatuses.includes(res.status),
  });
}

export function logFailure(response, label) {
  if (!shouldLogErrors()) {
    return;
  }

  const body = response && response.body ? response.body.slice(0, getMaxLogBody()) : '';
  const status = response ? response.status : 'n/a';
  const url = response && response.url ? response.url : 'n/a';

  console.error(`[${label}] status=${status} url=${url} body=${body}`);
}

export function buildOptions(testName) {
  const scenario = getScenarioName();

  const definitions = {
    smoke: {
      executor: 'per-vu-iterations',
      vus: 1,
      iterations: 1,
      maxDuration: '30s',
    },
    rps: buildRpsScenario(),
  };

  let scenarios = {};
  if (scenario === 'all') {
    scenarios = definitions;
  } else if (definitions[scenario]) {
    scenarios = { [scenario]: definitions[scenario] };
  } else {
    scenarios = { rps: definitions.rps };
  }

  const scenarioNames = Object.keys(scenarios);

  return {
    tags: {
      test: testName,
    },
    scenarios,
    thresholds: buildThresholds(scenarioNames),
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
  };
}

function buildRpsScenario() {
  const preAllocatedVUs = parseInt(__ENV.PREALLOCATED_VUS || '60', 10);
  const maxVUs = parseInt(__ENV.MAX_VUS || '200', 10);

  return {
    executor: 'ramping-arrival-rate',
    startRate: DEFAULT_WARMUP_RPS,
    timeUnit: '1s',
    preAllocatedVUs,
    maxVUs,
    stages: buildRpsStages(),
  };
}

function buildRpsStages() {
  const stages = [{ duration: DEFAULT_RPS_RAMP_DURATION, target: DEFAULT_WARMUP_RPS }];

  for (const target of DEFAULT_RPS_TARGETS) {
    stages.push({ duration: DEFAULT_RPS_RAMP_DURATION, target });
    stages.push({ duration: DEFAULT_RPS_HOLD_DURATION, target });
  }

  stages.push({ duration: DEFAULT_RPS_RAMP_DURATION, target: 0 });
  return stages;
}

function buildThresholds(scenarioNames) {
  const thresholds = {};

  for (const scenario of scenarioNames) {
    const tag = `{scenario:${scenario}}`;
    thresholds[`http_req_failed${tag}`] = ['rate<0.01'];
    thresholds[`http_req_duration${tag}`] = ['p(90)<3000', 'p(95)<3000', 'p(99)<3500'];
    thresholds[`slow_request_rate${tag}`] = ['rate<0.01'];
  }

  return thresholds;
}

function shouldLogErrors() {
  return __ENV.LOG_ERRORS !== 'false';
}

function getMaxLogBody() {
  const parsed = parseInt(__ENV.MAX_LOG_BODY || `${DEFAULT_LOG_BODY_LIMIT}`, 10);
  return Number.isFinite(parsed) ? parsed : DEFAULT_LOG_BODY_LIMIT;
}

function parseKeyword(value, fallback) {
  const parsed = parseInt(value ?? '', 10);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback;
}

function normalizeBaseUrl(value) {
  if (!value) {
    return DEFAULT_BASE_URL;
  }

  return value.endsWith('/') ? value.slice(0, -1) : value;
}
