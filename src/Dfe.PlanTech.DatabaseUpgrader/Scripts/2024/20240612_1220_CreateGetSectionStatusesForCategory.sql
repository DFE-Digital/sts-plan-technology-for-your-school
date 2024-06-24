-- TODO: Drop this on Dev after testing
-- TODO: Change this to CREATE OR ALTER PROCEDURE dbo.GetSectionStatuses and rename file

CREATE OR ALTER PROCEDURE dbo.GetSectionStatusesForCategory @categoryId nvarchar(30), @establishmentId int
AS

SELECT
    CurrentSubmission.sectionId,
    CurrentSubmission.completed,
    LastCompleteSubmission.maturity as lastMaturity,
    CurrentSubmission.dateCreated
FROM Contentful.Sections CS
-- The current submission
CROSS APPLY (
    SELECT TOP 1 sectionId, completed, dateCreated
    FROM [dbo].submission S
    WHERE
        CS.Id = S.sectionId
    AND S.establishmentId = @establishmentId
    AND S.deleted = 0
    ORDER BY
        S.dateCreated DESC
) CurrentSubmission
-- Use maturity from most recent complete submission (if there is one) so that user always sees recommendation
OUTER APPLY (
    SELECT TOP 1 maturity
    FROM [dbo].submission S
    WHERE
        CS.Id = S.sectionId
    AND S.establishmentId = @establishmentId
    AND S.deleted = 0
    AND s.completed = 1
    ORDER BY
        S.dateCreated DESC
) LastCompleteSubmission
WHERE
    CS.CategoryId = @categoryId
