ALTER PROCEDURE [dbo].[deleteDataForEstablishment]
  @establishmentRef nvarchar(50),
  @establishmentId BIGINT OUTPUT
AS
    BEGIN TRY
        SET NOCOUNT ON
        BEGIN TRAN
            DECLARE @establishmentId BIGINT;

            SELECT @establishmentId = [id]
            FROM [dbo].[establishment]
            WHERE [establishmentRef] = @establishmentRef;

            -- Delete the establishment recommendation history
            DELETE FROM [dbo].[establishmentRecommendationHistory]
            WHERE [establishmentId] = @establishmentId;

            -- Delete from the response table
            DELETE FROM [dbo].[response]
            WHERE [submissionId] IN (
                SELECT [id] FROM [dbo].[submission] WHERE [establishmentId] = @establishmentId
            );

            -- Delete from the submission table
            DELETE FROM [dbo].[submission]
            WHERE [establishmentId] = @establishmentId

        COMMIT TRAN
    END TRY
    BEGIN CATCH
	    ROLLBACK TRAN
        RETURN 0
    END CATCH
