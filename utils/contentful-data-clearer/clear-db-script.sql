DECLARE @Table NVARCHAR(100);
DECLARE @Schema NVARCHAR(100) = 'Contentful';

DECLARE @ConstraintsOff NVARCHAR(MAX) = ''
DECLARE @DeleteFromTables NVARCHAR(MAX) = ''
Declare @ConstraintsOn NVARCHAR(MAX) = ''

DECLARE Cur CURSOR FOR (
    SELECT SCHEMA_NAME(schema_id) + '.' + [name]
    FROM sys.tables
    WHERE
        SCHEMA_NAME(schema_id) = @Schema
    )
OPEN Cur
FETCH NEXT FROM Cur INTO @Table
WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ConstraintsOff = @ConstraintsOff + '
ALTER TABLE ' + @Table + ' NOCHECK CONSTRAINT ALL'
        SET @ConstraintsOn = @ConstraintsOn + '
ALTER TABLE ' + @Table + ' CHECK CONSTRAINT ALL'
        SET @DeleteFromTables = @DeleteFromTables + '
DELETE FROM ' + @Table
        FETCH NEXT FROM Cur INTO @Table
    END
CLOSE Cur
DEALLOCATE Cur

Exec sp_executesql @ConstraintsOff
Exec sp_executesql @DeleteFromTables
Exec sp_executesql @ConstraintsOn
