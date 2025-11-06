-- Source: src\Dfe.PlanTech.DatabaseUpgrader\Scripts\2024\20241009_1100_DboSchemaImprovements.sql (lines 54-79)
-- Fix: Set status to 'InProgress' when creating new submissions
-- Bug introduced in PR #1085 when status column was added but this procedure was not updated

ALTER PROCEDURE SelectOrInsertSubmissionId
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
                    (establishmentId, completed, sectionId, sectionName)
                VALUES
                    (@establishmentId, 0, @sectionId, @sectionName)

                SELECT @submissionId = SCOPE_IDENTITY()
            END
    COMMIT TRAN
END TRY
BEGIN CATCH
    ROLLBACK TRAN
END CATCH
GO
