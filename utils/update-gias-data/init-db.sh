#!/bin/bash
set -euo pipefail

/opt/mssql/bin/sqlservr &
sqlserver_pid=$!

for attempt in $(seq 1 60); do
  if /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" >/dev/null 2>&1; then
    break
  fi

  if ! kill -0 "$sqlserver_pid" 2>/dev/null; then
    echo "SQL Server failed to start" >&2
    exit 1
  fi

  sleep 2
done

/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "IF DB_ID(N'GIASData') IS NULL CREATE DATABASE [GIASData]"

sqlcmd_cmd=(/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d GIASData)

if ! "${sqlcmd_cmd[@]}" -Q "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'establishmentGroup'" | grep -q 1; then
  for script in \
    /usr/src/app/src/Dfe.PlanTech.DatabaseUpgrader/Scripts/2026/20260519_0930_AddGIASTables.sql \
    /usr/src/app/src/Dfe.PlanTech.DatabaseUpgrader/Scripts/2026/20260521_1415_AddGIASLinksTable.sql \
    /usr/src/app/src/Dfe.PlanTech.DatabaseUpgrader/Scripts/2026/20260803_1545_AddGiasTypesOfEstablishment.sql \
    /usr/src/app/src/Dfe.PlanTech.DatabaseUpgrader/Scripts/2026/20260827_1045_AddMoreGIASTables.sql; do
    echo "Applying $(basename "$script")"
    "${sqlcmd_cmd[@]}" -i "$script"
  done
fi

wait "$sqlserver_pid"
