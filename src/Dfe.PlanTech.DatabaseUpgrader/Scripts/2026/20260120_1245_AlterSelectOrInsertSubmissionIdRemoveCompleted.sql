ALTER PROCEDURE [dbo].[SelectOrInsertSubmissionId]
    @sectionId NVARCHAR(50),
    @sectionName NVARCHAR(50),
    @establishmentId INT,
    @submissionId INT OUTPUT
AS

BEGIN TRY
    BEGIN TRAN
        SELECT @submissionId = dbo.GetCurrentSubmissionId(@sectionId, @establishmentId)

        IF @submissionId IS NULL
            BEGIN
                INSERT INTO [dbo].[submission]
                    (establishmentId, sectionId, sectionName, status)
                VALUES
                    (@establishmentId, @sectionId, @sectionName, 'InProgress')

                SELECT @submissionId = SCOPE_IDENTITY()
            END
    COMMIT TRAN
END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH
GO
