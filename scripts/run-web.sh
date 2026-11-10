#!/usr/bin/env bash
set -euo pipefail
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
export ASPNETCORE_ENVIRONMENT=Development
export RaceDayApi__BaseUrl=http://localhost:5081/
exec dotnet run --project "$REPO_ROOT/Raceday part 3/RaceDay.Web/RaceDay.Web.csproj" --no-launch-profile --urls http://0.0.0.0:5080
