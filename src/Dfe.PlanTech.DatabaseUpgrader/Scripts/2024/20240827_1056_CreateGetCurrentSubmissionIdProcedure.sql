CREATE OR ALTER PROCEDURE GetCurrentSubmissionId
    @sectionId NVARCHAR(50),
    @establishmentId INT,
    @submissionId INT OUTPUT
AS

SELECT @submissionId = (
    SELECT TOP 1 Id
    FROM [dbo].[submission]
    WHERE completed = 0
        AND deleted = 0
        AND sectionId = @sectionId
        AND establishmentId = @establishmentId
    ORDER BY dateCreated DESC
)