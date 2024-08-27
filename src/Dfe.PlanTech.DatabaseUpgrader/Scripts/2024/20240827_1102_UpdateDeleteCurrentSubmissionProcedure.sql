CREATE OR ALTER PROCEDURE DeleteCurrentSubmission
    @sectionId NVARCHAR(50),
    @sectionName NVARCHAR(50),
    @establishmentId INT
AS
    BEGIN TRY
        DECLARE @submissionId INT

        EXEC GetCurrentSubmissionId
            @sectionid=@sectionId,
            @establishmentId=@establishmentId,
            @submissionId=@submissionId OUTPUT

        BEGIN TRAN
            UPDATE S
            SET deleted = 1
            FROM [dbo].[submission] S
            WHERE S.id = @submissionId
        COMMIT TRAN
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN
    END CATCH
GO
