IF OBJECT_ID(N'dbo.deleteDataForEstablishment', N'P') IS NOT NULL
    DROP PROCEDURE dbo.deleteDataForEstablishment
GO

CREATE PROCEDURE [dbo].[deleteDataForEstablishment] @establishmentRef nvarchar(50)
AS
    BEGIN TRY
        BEGIN TRAN

            DECLARE @establishmentUID int
            DECLARE @rowsDeletedCount int

            SELECT @establishmentUID = id FROM [dbo].[establishment] WHERE establishmentRef = @establishmentRef

            SELECT id, establishmentId
                INTO #submissionIds
                    FROM [dbo].[submission]
                    WHERE [submission].establishmentId = @establishmentUID

            DELETE FROM[dbo].[response]
            WHERE EXISTS (SELECT *
                            FROM #submissionIds
                            where [response].submissionId = #submissionIds.id)

            DELETE FROM [dbo].[submission] WHERE [submission].establishmentId = @establishmentUID

        COMMIT TRAN

        RETURN @@ROWCOUNT
    END TRY
    BEGIN CATCH
	    ROLLBACK TRAN
        RETURN 0
    END CATCH
