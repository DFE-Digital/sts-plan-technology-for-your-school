CREATE OR ALTER PROCEDURE [dbo].[deleteDataForEstablishment] @establishmentRef nvarchar(50)
AS
    BEGIN TRY
        BEGIN TRAN
            DECLARE @establishmentId BIGINT;

            SELECT @establishmentId = [id]
            FROM [dbo].[establishment]
            WHERE [establishmentRef] = @establishmentRef;

            -- Delete from the response table
            DELETE FROM [dbo].[response]
            WHERE [submissionId] IN (
                SELECT [id] FROM [dbo].[submission] WHERE [establishmentId] = @establishmentId
            );

            -- Delete from the submission table
            DELETE FROM [dbo].[submission]
            WHERE [establishmentId] = @establishmentId

        COMMIT TRAN

        RETURN @@ROWCOUNT
    END TRY
    BEGIN CATCH
	    ROLLBACK TRAN
        RETURN 0
    END CATCH
