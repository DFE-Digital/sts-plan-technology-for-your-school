ALTER PROCEDURE SelectOrInsertSubmissionId
    @sectionId NVARCHAR(50),
    @sectionName NVARCHAR(50),
    @establishmentId INT,
    @submissionId INT OUTPUT
AS

BEGIN TRY
    BEGIN TRAN
        EXEC GetCurrentSubmissionId
            @sectionid=@sectionId,
            @establishmentId=@establishmentId,
            @submissionId=@submissionId OUTPUT

        IF @submissionId IS NULL
            BEGIN
                INSERT INTO [dbo].[submission]
                    (establishmentId, completed, sectionId, sectionName)
                OUTPUT INSERTED.ID
                VALUES
                    (@establishmentId, 0, @sectionId, @sectionName)

                SELECT @submissionId = SCOPE_IDENTITY()
            END
    COMMIT TRAN
END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH
