#!/usr/bin/env bash
set -euo pipefail

name="${1:-azuresqledge}"
password="${2:?SA password required}"

# Strip any accidental Windows CR and surrounding whitespace
password="$(printf '%s' "$password_raw" | tr -d '\r' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')"

if [ -z "$password" ] || [ "${#password}" -lt 12 ]; then
  echo "❌ SA password is empty or too short (len=${#password})."
  exit 1
fi

# Clean any previous container with same name (idempotent for retries)
docker rm -f "$name" >/dev/null 2>&1 || true

# Start container
docker run --cap-add SYS_PTRACE \
  -e 'ACCEPT_EULA=1' \
  -e "MSSQL_SA_PASSWORD=$password" \
  -p 1433:1433 \
  --name "$name" -d \
  mcr.microsoft.com/azure-sql-edge:latest

echo "Waiting for readiness logs..."
max_retries=180
i=0
until docker logs "$name" 2>&1 | grep -q "SQL Server is now ready for client connections"; do
  sleep 1
  i=$((i+1))
  if [ "$i" -ge "$max_retries" ]; then
    echo "❌ Not ready in ${max_retries}s. Recent logs:"
    docker logs --tail 200 "$name" || true
    exit 1
  fi
done

echo "Waiting for SQL Edge to accept connections..."

# Retry up to 120s trying a trivial query (more reliable than log-grep)
max_retries=120
i=0
until docker exec "$name" /bin/bash -lc '/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" >/dev/null' ; do
  sleep 1
  i=$((i+1))
  if [ "$i" -ge "$max_retries" ]; then
    echo "❌ SQL Edge did not become ready within ${max_retries}s. Recent logs:"
    docker logs --tail 200 "$name" || true
    exit 1
  fi
done

echo "SQL Edge is ready. Creating database..."

# Create DB (no -it in CI)
docker exec "$name" /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P "$password" \
  -Q 'IF DB_ID(N"plantech-mock-db") IS NULL CREATE DATABASE [plantech-mock-db];'

# Configure user-secrets for the upgrader & seeders from the repo root
# (pin the project paths so working directory doesn't matter)
dotnet user-secrets --project ../../src/Dfe.PlanTech.DatabaseUpgrader init || true
dotnet user-secrets --project ../../src/Dfe.PlanTech.DatabaseUpgrader set \
  "ConnectionStrings:Database" \
  "Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=$password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=plantech-mock-db"

echo " DatabaseUpgrader..."
dotnet run --project ../../src/Dfe.PlanTech.DatabaseUpgrader \
  --connectionstring "Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=$password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=plantech-mock-db"

echo " test data..."
dotnet run --project ../../tests/Dfe.PlanTech.Web.SeedTestData

echo "✅ DB ready."
