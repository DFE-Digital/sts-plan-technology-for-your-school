#!/usr/bin/env bash
set -euo pipefail

# Configuration
CONTAINER_NAME="${1:-plantech-sql}"
SA_PASSWORD="${2:-${SA_PASSWORD:-}}"
SQL_IMAGE="mcr.microsoft.com/mssql/server:2025-latest"
SQL_PORT=1433
SQLCMD_PATH="/opt/mssql-tools18/bin/sqlcmd"
DATABASE_NAME="plantech-mock-db"
MAX_WAIT_SECONDS=180

DATABASE_UPGRADER_PROJECT="../../../src/Dfe.PlanTech.DatabaseUpgrader"
SEED_DATA_PROJECT="../../../tests/Dfe.PlanTech.Web.SeedTestData"

validate_password() {
    if [[ -z "$SA_PASSWORD" || ${#SA_PASSWORD} -lt 12 ]]; then
        echo "SA password is missing or too short."
        exit 1
    fi
}

start_sql_server() {
    echo "Starting SQL Server container: $CONTAINER_NAME"

    # Remove any existing container with the same name
    docker rm -f "$CONTAINER_NAME" >/dev/null 2>&1 || true

    docker run \
        --name "$CONTAINER_NAME" \
        --env "ACCEPT_EULA=Y" \
        --env "MSSQL_PID=Developer" \
        --env "MSSQL_SA_PASSWORD=$SA_PASSWORD" \
        --publish "$SQL_PORT:1433" \
        --detach \
        "$SQL_IMAGE"
}

execute_sql() {
    docker exec "$CONTAINER_NAME" \
        "$SQLCMD_PATH" \
        -S localhost \
        -U sa \
        -P "$SA_PASSWORD" \
        -C \
        -b \
        -h -1 \
        -W \
        -Q "$1"
}

wait_for_sql_server() {
    echo "Waiting for SQL Server to become ready..."

    for ((attempt = 1; attempt <= MAX_WAIT_SECONDS; attempt++)); do
        if execute_sql "SELECT 1;" >/dev/null 2>&1; then
            echo "SQL Server is ready."
            return 0
        fi

        sleep 1
    done

    echo "SQL Server failed to become ready."
    docker logs --tail 200 "$CONTAINER_NAME" || true
    exit 1
}

create_database() {
    echo "Creating database: $DATABASE_NAME"

    execute_sql "
        IF DB_ID(N'$DATABASE_NAME') IS NULL
            CREATE DATABASE [$DATABASE_NAME];
    "

    echo "Database created successfully."
}

verify_database() {
    echo "Verifying database: $DATABASE_NAME"

    execute_sql "
        IF DB_ID(N'$DATABASE_NAME') IS NULL
            THROW 50000, 'Database does not exist.', 1;
    "

    echo "Database verified successfully."
}

build_connection_string() {
    CONNECTION_STRING="Server=tcp:localhost,$SQL_PORT;"
    CONNECTION_STRING+="Database=$DATABASE_NAME;"
    CONNECTION_STRING+="User ID=sa;"
    CONNECTION_STRING+="Password=$SA_PASSWORD;"
    CONNECTION_STRING+="Persist Security Info=False;"
    CONNECTION_STRING+="MultipleActiveResultSets=False;"
    CONNECTION_STRING+="Encrypt=True;"
    CONNECTION_STRING+="TrustServerCertificate=True;"
    CONNECTION_STRING+="Connection Timeout=30;"
    CONNECTION_STRING+="Max Pool Size=1000;"
}

run_dotnet_project() {
    local project_path="$1"

    dotnet user-secrets --project "$project_path" init || true

    dotnet user-secrets --project "$project_path" set \
        "ConnectionStrings:Database" "$CONNECTION_STRING"

    dotnet run --project "$project_path" \
        --connectionstring "$CONNECTION_STRING"
}

upgrade_database() {
    echo "Running DatabaseUpgrader..."
    run_dotnet_project "$DATABASE_UPGRADER_PROJECT"
}

seed_database() {
    echo "Seeding test data..."
    run_dotnet_project "$SEED_DATA_PROJECT"
}

main() {
    validate_password
    start_sql_server
    wait_for_sql_server
    create_database
    verify_database
    build_connection_string
    upgrade_database
    seed_database

    echo "Database is ready for E2E tests."
}

main
