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
      AND status = 'InProgress'
    ORDER BY Id DESC;

    RETURN @submissionId;
END
GO