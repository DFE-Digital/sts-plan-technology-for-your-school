name=$1
password=$2

docker run --cap-add SYS_PTRACE -e 'ACCEPT_EULA=1' -e "MSSQL_SA_PASSWORD=$password" -p 1433:1433 --name $name -d mcr.microsoft.com/azure-sql-edge

docker exec -it "$name" /opt/mssql-tools/bin/sqlcmd \
          -S localhost -U SA -P "$password" \
          -Q 'CREATE DATABASE [plantech-mock-db]'

dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Database" "Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=$password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=plantech-mock-db"

cd ../../src/Dfe.PlanTech.DatabaseUpgrader || exit

dotnet run --connectionstring "Server=tcp:localhost,1433;Persist Security Info=False;User ID=sa;Password=$password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Max Pool Size=1000;Database=plantech-mock-db"
