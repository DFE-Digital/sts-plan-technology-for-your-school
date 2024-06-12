ALTER PROCEDURE GetSectionStatuses
    @splitStringVal NVARCHAR(255),
    @establishmentId INT
AS

SELECT value AS sectionId
INTO #SectionIds
FROM STRING_SPLIT(@splitStringVal, ',')

SELECT
    CurrentSubmission.sectionId,
    CAST(CurrentSubmission.completed AS INT) completed,
    LastCompleteSubmission.maturity as lastMaturity,
    CurrentSubmission.dateCreated
From #SectionIds Ids
-- The current submission
CROSS APPLY (
    SELECT TOP 1 sectionId, completed, dateCreated
    FROM [dbo].submission S
    WHERE
        Ids.sectionId = S.sectionId
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
        Ids.sectionId = S.sectionId
    AND S.establishmentId = @establishmentId
    AND S.deleted = 0
    AND s.completed = 1
    ORDER BY
        S.dateCreated DESC
) LastCompleteSubmission
