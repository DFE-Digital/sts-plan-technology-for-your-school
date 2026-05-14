SET NOCOUNT ON;
SET XACT_ABORT ON;

IF COL_LENGTH(N'dbo.submission', N'completed') IS NOT NULL
BEGIN
    DECLARE @df sysname;
    DECLARE @sql nvarchar(max);

    SELECT @df = dc.name
    FROM sys.default_constraints AS dc
    INNER JOIN sys.columns AS c
        ON c.object_id = dc.parent_object_id
       AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.submission', N'U')
      AND c.name = N'completed';

    IF @df IS NOT NULL
    BEGIN
        SET @sql = N'ALTER TABLE dbo.submission DROP CONSTRAINT ' + QUOTENAME(@df) + N';';
        EXEC sys.sp_executesql @sql;
    END

    ALTER TABLE dbo.submission DROP COLUMN completed;
END
GO