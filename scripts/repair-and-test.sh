#!/usr/bin/env bash
# RaceDay Part 3 emergency Codespaces launcher and login test.
# Uses the existing SQL Server Docker container and original EF migrations.
# Stop any old API/web processes in their terminal tabs before running.
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

for port in 5080 5081; do
  if python3 - "$port" <<'PYPORT'
import socket, sys
s = socket.socket()
s.settimeout(1)
try:
    s.connect(('127.0.0.1', int(sys.argv[1])))
    sys.exit(0)
except OSError:
    sys.exit(1)
finally:
    s.close()
PYPORT
  then
    echo "ERROR: port $port is already in use by an older RaceDay process." >&2
    echo 'Find the earlier API/web terminal tab, press Ctrl+C, and rerun this command.' >&2
    exit 1
  fi
done

echo '1/5 Building the existing Part 3 solution...'
dotnet build 'Raceday part 3/Raceday.API.sln' --nologo

echo '2/5 Setting up SQL Server and applying migrations...'
bash scripts/setup-codespaces.sh

echo '3/5 Starting the API on 5081 and MVC website on 5080...'
nohup bash scripts/run-api.sh > .raceday-api.log 2>&1 < /dev/null &
API_PID=$!
nohup bash scripts/run-web.sh > .raceday-web.log 2>&1 < /dev/null &
WEB_PID=$!

for i in $(seq 1 35); do
  api=0; web=0
  curl -sS --max-time 2 -o /dev/null http://127.0.0.1:5081/swagger/index.html 2>/dev/null && api=1
  curl -fsS --max-time 2 -o /dev/null http://127.0.0.1:5080/ 2>/dev/null && web=1
  if [[ "$api" == 1 && "$web" == 1 ]]; then break; fi
  if ! kill -0 "$API_PID" 2>/dev/null || ! kill -0 "$WEB_PID" 2>/dev/null; then break; fi
  sleep 2
done
if ! curl -fsS --max-time 4 -o /dev/null http://127.0.0.1:5081/swagger/index.html || ! curl -fsS --max-time 4 -o /dev/null http://127.0.0.1:5080/; then
  echo 'ERROR: API or website failed to start. Diagnostic logs:' >&2
  tail -45 .raceday-api.log >&2 || true
  tail -45 .raceday-web.log >&2 || true
  exit 1
fi

echo '4/5 Registering a disposable test account DIRECTLY against the API...'
TEST_EMAIL="raceday-api-test-$(date +%s)-${RANDOM}@example.com"
PAYLOAD="$(printf '{"firstName":"API","lastName":"Test","email":"%s","password":"RaceDay!2026","role":"Participant"}' "$TEST_EMAIL")"
HTTP_CODE="$(curl -sS --max-time 25 -o /tmp/raceday-api-register-response.json -w '%{http_code}' -X POST http://127.0.0.1:5081/api/auth/register -H 'Content-Type: application/json' --data "$PAYLOAD")"
if [[ "$HTTP_CODE" != 201 ]]; then
  echo "ERROR: Registration endpoint returned HTTP $HTTP_CODE." >&2
  echo 'API response (up to 1500 bytes):' >&2
  head -c 1500 /tmp/raceday-api-register-response.json >&2 || true
  echo >&2
  echo 'Recent API errors (may explain connection or migration issues):' >&2
  tail -60 .raceday-api.log >&2 || true
  echo 'Please share the output above so this can be fixed precisely.' >&2
  exit 1
fi

echo 'PASS: API registration returned HTTP 201.'
echo '5/5 Testing MVC registration and login for Participant and Organiser...'
if ! python3 scripts/smoke-test.py; then
  echo 'Recent API log:' >&2
  tail -55 .raceday-api.log >&2 || true
  echo 'Recent web log:' >&2
  tail -35 .raceday-web.log >&2 || true
  exit 1
fi

echo 'SUCCESS: RaceDay Part 3 tests passed. Website: open Codespaces Ports > 5080.'
echo 'Logs: .raceday-api.log and .raceday-web.log (excluded from Git).'
