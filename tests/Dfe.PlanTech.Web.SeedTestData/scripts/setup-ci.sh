#!/usr/bin/env bash
set -euo pipefail

name="${1:-azuresqledge}"
password_raw="${2-}"
# Fall back to env var if 2nd arg not provided
if [ -z "${password_raw:-}" ] && [ -n "${SA_PASSWORD-}" ]; then password_raw="$SA_PASSWORD"; fi
password="$(printf '%s' "${password_raw:-}" | tr -d '\r' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')"
if [ -z "$password" ] || [ "${#password}" -lt 12 ]; then
  echo "SA password missing/too short"
  exit 1
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
    echo "Not ready in ${max_retries}s. Recent logs:"
    docker logs --tail 200 "$name" || true
    exit 1
  fi
done

# pick sqlcmd path in container (+ TLS switch for tools18)
choose_sqlcmd='
  if [ -x /opt/mssql-tools/bin/sqlcmd ]; then
    echo "/opt/mssql-tools/bin/sqlcmd";
  elif [ -x /opt/mssql-tools18/bin/sqlcmd ]; then
    echo "/opt/mssql-tools18/bin/sqlcmd";
  else
    echo "";
  fi
'

# sanity: show pw length inside container (no secret leak)
len_in_container=$(docker exec "$name" /bin/bash -lc 'echo -n "${MSSQL_SA_PASSWORD}" | wc -c' || echo 0)
echo "Container sees MSSQL_SA_PASSWORD length: $len_in_container"

sqlcmd_path=$(docker exec "$name" /bin/bash -lc "$choose_sqlcmd")
if [ -z "$sqlcmd_path" ]; then
  echo "sqlcmd not found in container at /opt/mssql-tools{,18}/bin/sqlcmd"
  docker logs --tail 200 "$name" || true
  exit 1
fi
echo "Using sqlcmd: $sqlcmd_path"

# if using tools18, add -C to trust server cert
sqlcmd_tls_flag=""
if [[ "$sqlcmd_path" == *"mssql-tools18"* ]]; then
  sqlcmd_tls_flag="-C"
fi

echo "SQL Edge ready. Creating database..."

# Create DB if missing (no shell in container; pass host $password directly)
docker exec "$name" \
  /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P "$password" -b -h -1 -W -Q \
  "IF DB_ID(N'plantech-mock-db') IS NULL CREATE DATABASE [plantech-mock-db];"

# Verify DB exists before proceeding (retry a few seconds)
echo "Verifying database creation..."
verify_retries=20
until docker exec "$name" \
  /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P "$password" -b -h -1 -W -Q \
  "SET NOCOUNT ON; SELECT name FROM sys.databases WHERE name = N'plantech-mock-db';" \
  | tr -d '\r' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//' | grep -x 'plantech-mock-db' >/dev/null; do
  sleep 1
  verify_retries=$((verify_retries-1))
  if [ $verify_retries -le 0 ]; then
    echo "Database 'plantech-mock-db' not found after create. Recent logs:"
    docker logs --tail 200 "$name" || true
    exit 1
  fi
done
echo "Database present."


conn_str="Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=$password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=plantech-mock-db"

echo "Writing user-secrets for DatabaseUpgrader..."
dotnet user-secrets --project ../../../src/Dfe.PlanTech.DatabaseUpgrader init || true
dotnet user-secrets --project ../../../src/Dfe.PlanTech.DatabaseUpgrader set \
  "ConnectionStrings:Database" "$conn_str"

echo "Running DatabaseUpgrader..."
dotnet run --project ../../../src/Dfe.PlanTech.DatabaseUpgrader \
  --connectionstring "$conn_str"


dotnet user-secrets --project ../../../tests/Dfe.PlanTech.Web.SeedTestData init || true
dotnet user-secrets --project ../../../tests/Dfe.PlanTech.Web.SeedTestData set \
  "ConnectionStrings:Database" "$conn_str"


echo "Seeding test data..."
dotnet run --project ../../../tests/Dfe.PlanTech.Web.SeedTestData --connectionstring "$conn_str"

echo "DB ready."
