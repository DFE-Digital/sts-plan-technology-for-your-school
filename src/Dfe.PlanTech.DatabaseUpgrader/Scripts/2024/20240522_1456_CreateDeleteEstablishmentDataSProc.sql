CREATE OR ALTER PROCEDURE [dbo].[deleteDataForEstablishment] @establishmentRef nvarchar(50)
AS
    BEGIN TRY
        BEGIN TRAN
            DELETE R
            FROM [dbo].[response] R
            JOIN [dbo].[submission] S ON R.[submissionId] = S.[id]
            WHERE [S].[establishmentId] = (SELECT [id] FROM [dbo].[establishment] WHERE establishmentRef = @establishmentRef)

        COMMIT TRAN

        RETURN @@ROWCOUNT
    END TRY
    BEGIN CATCH
	    ROLLBACK TRAN
        RETURN 0
    END CATCH