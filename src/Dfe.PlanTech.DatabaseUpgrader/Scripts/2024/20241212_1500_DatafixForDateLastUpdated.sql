ALTER TABLE dbo.submission DISABLE TRIGGER tr_submission
GO

UPDATE S
-- when the submission most recently had a response created, or fallback on the submission creation time
SET dateLastUpdated = ISNULL(S.dateCompleted, ISNULL(LASTRESPONSE.dateCreated, S.dateCreated))
FROM dbo.submission S
CROSS APPLY (
    SELECT TOP 1
        R.dateCreated
    FROM [dbo].response R
    WHERE
        R.submissionId = S.id
    ORDER BY R.dateCreated DESC
) LastResponse
WHERE
    -- Fix the updates that happened at 2024-12-12T10:05:14.7700000
    dateLastUpdated BETWEEN DATETIMEFROMPARTS(2024, 12, 12, 10, 5, 14, 0)
                        AND DATETIMEFROMPARTS(2024, 12, 12, 10, 5, 15, 0)

GO

ALTER TABLE dbo.submission ENABLE TRIGGER tr_submission
GO

ALTER PROCEDURE dbo.GetSectionStatuses @sectionIds NVARCHAR(MAX), @establishmentId INT
AS

SELECT
    Value sectionId
INTO #SectionIds
FROM STRING_SPLIT(@sectionIds, ',')

SELECT
    CurrentSubmission.sectionId,
    CurrentSubmission.completed,
    LastCompleteSubmission.maturity   AS lastMaturity,
    CurrentSubmission.dateCreated,
    ISNULL(CurrentSubmission.dateCompleted, CurrentSubmission.dateLastUpdated) as dateUpdated,
    LastCompleteSubmission.viewed
FROM #SectionIds SI
-- The current submission
CROSS APPLY (
    SELECT TOP 1
        sectionId,
        completed,
        S.id,
        dateCreated,
        dateLastUpdated,
        dateCompleted
    FROM [dbo].submission S
    WHERE
          SI.sectionId = S.sectionId
      AND S.establishmentId = @establishmentId
      AND S.deleted = 0
    ORDER BY S.dateCreated DESC
) CurrentSubmission
-- Use maturity from most recent complete submission (if there is one) so that user always sees recommendation
OUTER APPLY (
    SELECT TOP 1
        maturity,
        viewed
    FROM [dbo].submission S
    WHERE
          SI.sectionId = S.sectionId
      AND S.establishmentId = @establishmentId
      AND S.deleted = 0
      AND s.completed = 1
    ORDER BY S.dateCreated DESC
) LastCompleteSubmission
