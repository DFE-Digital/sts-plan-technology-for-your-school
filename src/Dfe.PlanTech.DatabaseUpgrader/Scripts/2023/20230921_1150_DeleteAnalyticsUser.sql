-- $Analytics_Username$ $Analytics_Password$
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = '$Analytics_Username$')
BEGIN
    DROP USER [$Analytics_Username$]
END