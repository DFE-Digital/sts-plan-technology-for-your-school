DECLARE @df sysname;

SELECT @df = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c
  ON c.object_id = dc.parent_object_id
 AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID('dbo.submission')
  AND c.name = 'completed';

IF @df IS NOT NULL
    EXEC('ALTER TABLE dbo.submission DROP CONSTRAINT [' + @df + ']');

ALTER TABLE dbo.submission DROP COLUMN completed;

GO