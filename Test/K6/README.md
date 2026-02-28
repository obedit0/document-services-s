# k6 Signature Controller

Scripts k6 por endpoint para `SignatureController` con umbral de latencia de 3s.

## Variables de entorno

Requeridas:
- `BASE_URL`: URL base del API. Ejemplo: `http://localhost:5000`.
- `TOKEN`: JWT Bearer. Si el entorno no requiere auth, puedes omitirlo.

Requeridas por endpoint:
- `CHANNEL_IDENTIFICATION`: header `channelIdentification` (default: `1`).
- `KEYWORD`: keyword existente (int) para `retrieve`, `control`, `documents`, `completed`, `documents-persisted` (default: `1000`). Se usa tambien como `idFirma` en actualizaciones. En `initiate` se usa como base para generar un keyword unico por iteracion.

Opcionales:
- `SCENARIO`: `rps` (default), `smoke`, `all`.
- `PREALLOCATED_VUS`: VUs iniciales para `ramping-arrival-rate` (default: `60`).
- `MAX_VUS`: VUs maximos para `ramping-arrival-rate` (default: `200`).
- `LOG_ERRORS`: `false` para silenciar logs de errores (default: `true`).
- `MAX_LOG_BODY`: limite de caracteres al loguear body (default: `500`).
- `SLEEP`: espera entre iteraciones. Default `0` para `rps`, `1` para `smoke`.

## Escenarios

### rps (ramping-arrival-rate)

Warm-up y etapas de RPS (ramp + hold):
- warm-up: 5 RPS por 30s
- 50 RPS (30s ramp + 1m hold)
- ramp down: 0 RPS por 30s

### smoke

1 VU, 1 iteracion.

## Comandos de ejecucion (PowerShell)

Initiate (genera keyword unico por iteracion):
```powershell
$env:BASE_URL = "http://localhost:5000"
$env:TOKEN = "<token>"
$env:CHANNEL_IDENTIFICATION = "1"
$env:SCENARIO = "rps"

k6 run tests/performance/k6/signature/initiate.k6.js
```

Retrieve (requiere keyword existente):
```powershell
$env:KEYWORD = "123456"
k6 run tests/performance/k6/signature/retrieve.k6.js
```

Smoke:
```powershell
$env:SCENARIO = "smoke"
k6 run tests/performance/k6/signature/retrieve.k6.js
```

## Endpoints cubiertos

- `POST /document-services-s/v1/signature-request/initiate`
- `GET /document-services-s/v1/signature-request/retrieve?keyword={keyword}`
- `PUT /document-services-s/v1/signature-request/documents-persisted`
- `PUT /document-services-s/v1/signature-request/completed`
- `PUT /document-services-s/v1/signature-request/control?keyword={keyword}`
- `GET /document-services-s/v1/signature-request/documents?keyword={keyword}`
