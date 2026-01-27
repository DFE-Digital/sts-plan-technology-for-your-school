IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'AnalyticsUser')
BEGIN
    DROP USER [AnalyticsUser]
END
