CREATE OR ALTER PROCEDURE DeleteCurrentSubmission
    @sectionId NVARCHAR(50),
    @sectionName NVARCHAR(50),
    @establishmentId INT
AS
    BEGIN TRY
        Declare @SubmissionId Int
        Select @SubmissionId = Id
            From [dbo].submission
            WHERE
                completed = 0
            and deleted = 0
            and sectionId = @sectionId
            and sectionName = @sectionName
            and establishmentId = @establishmentId

        BEGIN TRAN
            UPDATE S
            SET Deleted = 1
            FROM [dbo].[submission] S
            WHERE S.id = @SubmissionId
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN
    END CATCH
GO
