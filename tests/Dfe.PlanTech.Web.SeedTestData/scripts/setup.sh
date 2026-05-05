set -euo pipefail

name=$1
password=$2

docker run \
  --cap-add SYS_PTRACE \
  -e 'ACCEPT_EULA=1' \
  -e "MSSQL_SA_PASSWORD=$password" \
  -p 1433:1433 \
  --name $name \
  -d mcr.microsoft.com/mssql/server:latest

echo "Waiting for SQL Server to accept connections..."
until docker exec "$name" //opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$password" -C -l 1 \
  -Q "SELECT 1" >/dev/null 2>$1; do
  sleep 1
done

echo "SQL Server is now ready for client connections"

docker exec "$name" //opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "$password" -C \
  -b -Q 'CREATE DATABASE [plantech-mock-db]'

CONN="Server=tcp:localhost,1433;User ID=sa;Password=$password;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=plantech-mock-db"

dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Database" "$CONN"

cd ../../src/Dfe.PlanTech.DatabaseUpgrader && dotnet run --connectionstring "$CONN"
cd ../../tests/Dfe.PlanTech.Web.SeedTestData && dotnet run
