CREATE OR ALTER PROCEDURE ResetSubmissionKGTest
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
            and sectionId = @sectionId
            and sectionName = @sectionName
            and establishmentId = @establishmentId

        BEGIN TRAN
            DELETE R
            from [dbo].[response] R
            JOIN [dbo].[submission] S on R.submissionId = S.id
            WHERE S.id = @SubmissionId

            DELETE S
            FROM [dbo].[submission] S
            WHERE S.id = @SubmissionId
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN
    END CATCH
GO
