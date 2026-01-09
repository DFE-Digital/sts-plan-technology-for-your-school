CREATE PROCEDURE SelectOrInsertSubmissionId
  @sectionId NVARCHAR(50),
  @sectionName NVARCHAR(50),
  @establishmentId INT,
  @submissionId INT OUTPUT
AS

  BEGIN TRY
    BEGIN TRAN

    SELECT @submissionId = (SELECT
                    TOP 1
                    Id from [dbo].[submission]
                    WHERE completed = 0
                      AND sectionId = @sectionId
                      AND sectionName = @sectionName
                      AND establishmentId = @establishmentId
                    ORDER BY dateCreated DESC)

    IF @submissionId IS NULL
      BEGIN
        INSERT INTO [dbo].[submission]
          (establishmentId, completed, sectionId, sectionName)
        output INSERTED.ID
        VALUES
          (@establishmentId, 0, @sectionId, @sectionName)

        SELECT @submissionId = Scope_Identity()
      END
    COMMIT TRAN
  END TRY
  BEGIN CATCH
    ROLLBACK TRAN
  END CATCH
