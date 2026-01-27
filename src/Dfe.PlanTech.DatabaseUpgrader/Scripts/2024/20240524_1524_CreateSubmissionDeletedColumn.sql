If Not Exists (
    SELECT 1
    FROM sys.columns C
    JOIN sys.tables T on C.object_id = T.object_id
    JOIN sys.schemas S on T.schema_id = S.schema_id
    WHERE
        S.name = 'dbo'
    AND T.name = 'submission'
    AND C.name = 'deleted'
)
BEGIN
    BEGIN TRY
        BEGIN TRAN
            ALTER TABLE [dbo].[submission] ADD deleted BIT NOT NULL DEFAULT 0
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN
    END CATCH
END
