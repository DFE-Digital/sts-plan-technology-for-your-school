-- $Analytics_Username$ $Analytics_Password$
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = [$Analytics_Username$])
BEGIN
    CREATE USER [$Analytics_Username$] WITH PASSWORD = '$Analytics_Password$'
    ALTER ROLE [db_datareader] ADD MEMBER [$Analytics_Username$]
    GRANT SELECT ON SCHEMA :: [dbo] to [AnalyticsUser];
END