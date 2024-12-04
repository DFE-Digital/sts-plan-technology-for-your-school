-- Add viewed column to submission table and set it to true for existing records

ALTER TABLE dbo.submission
ADD viewed BIT NOT NULL DEFAULT 0

GO

UPDATE dbo.submission
SET viewed = 1
WHERE viewed = 0

GO

-- Return viewed in the section statuses procedure

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
    CurrentSubmission.dateLastUpdated AS dateUpdated,
    LastCompleteSubmission.viewed
FROM #SectionIds SI
-- The current submission
CROSS APPLY (
    SELECT TOP 1
        sectionId,
        completed,
        S.id,
        dateCreated,
        dateLastUpdated
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

DROP TABLE #SectionIds

GO
