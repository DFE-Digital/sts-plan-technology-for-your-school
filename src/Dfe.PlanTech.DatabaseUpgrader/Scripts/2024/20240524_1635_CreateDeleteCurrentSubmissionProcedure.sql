CREATE OR ALTER PROCEDURE DeleteCurrentSubmission
    @sectionId NVARCHAR(50),
    @sectionName NVARCHAR(50),
    @establishmentId INT
AS
    BEGIN TRY
        DECLARE @submissionId INT
        SELECT @submissionId = Id
            FROM [dbo].[submission]
            WHERE
                completed = 0
            AND deleted = 0
            AND sectionId = @sectionId
            AND sectionName = @sectionName
            AND establishmentId = @establishmentId

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
