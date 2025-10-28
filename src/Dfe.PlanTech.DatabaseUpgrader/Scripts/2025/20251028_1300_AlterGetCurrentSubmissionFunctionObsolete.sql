ALTER FUNCTION [dbo].[GetCurrentSubmissionId]
(
    @sectionId NVARCHAR(50),
    @establishmentId INT
)
RETURNS INT
AS
BEGIN
    DECLARE @submissionId INT;

    SELECT TOP 1 @submissionId = Id
    FROM [dbo].[submission]
    WHERE sectionId = @sectionId
      AND establishmentId = @establishmentId
      AND completed = 0
      AND (status IS NULL OR status <> 'Inaccessible' OR status <> 'Obsolete') -- Exclude inaccessible and obsolete submissions
    ORDER BY Id DESC;

    RETURN @submissionId;
END
GO
