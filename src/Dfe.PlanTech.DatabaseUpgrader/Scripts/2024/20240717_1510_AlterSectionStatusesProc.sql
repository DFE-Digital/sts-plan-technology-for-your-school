ALTER PROCEDURE dbo.GetSectionStatuses @categoryId nvarchar(30), @establishmentId int
AS

SELECT
    CurrentSubmission.sectionId,
    CurrentSubmission.completed,
    LastCompleteSubmission.maturity as lastMaturity,
    CurrentSubmission.dateCreated,
    -- lastUpdated date of the submission is:
    -- when submission was finished (if its done)
    -- OR an answer in the submission was added/edited (if questions answered but submission not finished)
    -- OR submission was created (if nothing answered yet)
    ISNULL(
        CurrentSubmission.dateCompleted,
        ISNULL(LastResponse.dateCreated, CurrentSubmission.dateCreated)
    ) AS dateUpdated
FROM Contentful.Sections CS
-- The current submission
CROSS APPLY (
    SELECT TOP 1 sectionId, completed, dateCreated, S.id, dateCompleted
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
-- Use the created time of the most recent response to get the last updated time of the submission
CROSS APPLY (
    SELECT TOP 1 R.dateCreated
    FROM [dbo].response R
    WHERE
        R.submissionId = CurrentSubmission.id
    ORDER BY
        R.dateCreated DESC
) LastResponse
WHERE
    CS.CategoryId = @categoryId
