#!/usr/bin/env bash
set -euo pipefail

name="${1:-azuresqledge}"
password_raw="${2-}"
# Fall back to env var if 2nd arg not provided
if [ -z "${password_raw:-}" ] && [ -n "${SA_PASSWORD-}" ]; then password_raw="$SA_PASSWORD"; fi
password="$(printf '%s' "${password_raw:-}" | tr -d '\r' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')"
if [ -z "$password" ] || [ "${#password}" -lt 12 ]; then
  echo "❌ SA password missing/too short"; exit 1
fi

# idempotent cleanup + start
docker rm -f "$name" >/dev/null 2>&1 || true
docker run --cap-add SYS_PTRACE \
  -e 'ACCEPT_EULA=1' \
  -e "MSSQL_SA_PASSWORD=$password" \
  -p 1433:1433 \
  --name "$name" -d mcr.microsoft.com/azure-sql-edge:latest

echo "Waiting for readiness logs..."
max_retries=180; i=0
until docker logs "$name" 2>&1 | grep -q "SQL Server is now ready for client connections"; do
  sleep 1; i=$((i+1))
  if [ "$i" -ge "$max_retries" ]; then
    echo "❌ Not ready in ${max_retries}s. Recent logs:"; docker logs --tail 200 "$name" || true; exit 1
  fi
done

echo "SQL Edge ready. Creating database..."
docker exec "$name" /bin/bash -lc '
  /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$MSSQL_SA_PASSWORD" \
    -Q "IF DB_ID(N''plantech-mock-db'') IS NULL CREATE DATABASE [plantech-mock-db];"
' || docker exec "$name" /bin/bash -lc '
  # fallback if tools are under tools18 in some images
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "$MSSQL_SA_PASSWORD" -C \
    -Q "IF DB_ID(N''plantech-mock-db'') IS NULL CREATE DATABASE [plantech-mock-db];"
'

echo "Writing user-secrets for DatabaseUpgrader..."
dotnet user-secrets --project ../../../src/Dfe.PlanTech.DatabaseUpgrader init || true
dotnet user-secrets --project ../../../src/Dfe.PlanTech.DatabaseUpgrader set \
  "ConnectionStrings:Database" \
  "Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=$password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=plantech-mock-db"

echo "Running DatabaseUpgrader..."
dotnet run --project ../../../src/Dfe.PlanTech.DatabaseUpgrader \
  --connectionstring "Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=$password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=plantech-mock-db"

echo "Seeding test data..."
dotnet run --project ../../../tests/Dfe.PlanTech.Web.SeedTestData

echo "DB ready."
