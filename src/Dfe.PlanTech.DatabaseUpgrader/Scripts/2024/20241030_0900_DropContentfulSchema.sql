DECLARE @Schema   NVARCHAR(256) = 'Contentful'
DECLARE @SchemaId INT
DECLARE @SQL      NVARCHAR(MAX) = ''

SELECT
    @SchemaId = schema_id
FROM sys.schemas
WHERE
    name = @Schema

-- SQL to Delete all views
SELECT
    @SQL = @SQL + 'DROP VIEW ' + @Schema + '.[' + Name + ']
'
FROM sys.views V
WHERE
    V.schema_id = @SchemaId

-- SQL to Delete all stored procedures
SELECT
    @SQL = @SQL + 'DROP PROCEDURE ' + @Schema + '.[' + Name + ']
'
FROM sys.procedures SP
WHERE
    SP.schema_id = @SchemaId

-- SQL to delete all functions
SELECT
    @SQL = @SQL + 'DROP FUNCTION ' + @Schema + '.[' + Name + ']
'
FROM sys.objects SO
WHERE
      SO.schema_id = @SchemaId
  AND SO.type IN ('FN', 'IF', 'TF')

-- SQL to Delete all Foreign Keys
SELECT
    @SQL = @SQL + 'ALTER TABLE ' + @Schema + '.[' + OBJECT_NAME(FK.parent_object_id) + '] DROP CONSTRAINT ' + Name + '
'
FROM sys.foreign_keys FK
WHERE
    FK.schema_id = @SchemaId

-- SQL to Delete all triggers
SELECT
    @SQL = @SQL + 'DROP TRIGGER ' + @Schema + '.[' + TR.name + ']
'
FROM sys.triggers TR
JOIN sys.objects SO ON TR.object_id = SO.object_id
WHERE
    SO.schema_id = @SchemaId

-- SQL to Delete all Tables
SELECT
    @SQL = @SQL + 'DROP TABLE ' + @Schema + '.[' + Name + ']
'
FROM sys.tables ST
WHERE
    ST.schema_id = @SchemaId

BEGIN TRAN
BEGIN TRY
    EXEC sp_executesql @SQL
    DROP SCHEMA Contentful
    COMMIT TRAN
END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH
