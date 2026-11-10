#!/usr/bin/env bash
set -euo pipefail
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$REPO_ROOT/.raceday-codespaces.env"
if [[ ! -f "$ENV_FILE" ]]; then
  echo 'Run bash scripts/setup-codespaces.sh first.' >&2
  exit 1
fi
# shellcheck disable=SC1090
source "$ENV_FILE"
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__RaceDayConnection="Server=127.0.0.1,1433;Database=RaceDayPart3DB;User Id=sa;Password=${MSSQL_SA_PASSWORD};Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
exec dotnet run --project "$REPO_ROOT/Raceday part 3/Raceday.API.csproj" --no-launch-profile --urls http://0.0.0.0:5081
