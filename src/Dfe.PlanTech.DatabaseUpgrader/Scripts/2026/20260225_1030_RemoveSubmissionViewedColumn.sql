BEGIN
	DECLARE @ConstraintName sysname;

	SELECT @ConstraintName = dc.name
	FROM sys.default_constraints dc
	JOIN sys.columns c
		ON c.default_object_id = dc.object_id
	WHERE dc.parent_object_id = OBJECT_ID(N'dbo.submission')
	  AND c.name = N'viewed';

	IF @ConstraintName IS NOT NULL
		EXEC(N'ALTER TABLE dbo.submission DROP CONSTRAINT [' + @ConstraintName + N']');

	ALTER TABLE dbo.submission DROP COLUMN viewed;
END