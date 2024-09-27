-- Set missing updated date values

UPDATE dbo.[user]
SET dateLastUpdated = dateCreated
WHERE
    dateLastUpdated IS NULL

UPDATE dbo.[establishment]
SET dateLastUpdated = dateCreated
WHERE
    dateLastUpdated IS NULL

UPDATE dbo.[response]
SET dateLastUpdated = dateCreated
WHERE
    dateLastUpdated IS NULL

UPDATE S
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
    dateLastUpdated IS NULL

GO

-- Add defaults to all cols
ALTER TABLE dbo.[user]
    ADD CONSTRAINT DF_user_dateLastUpdated DEFAULT GETUTCDATE() FOR dateLastUpdated
ALTER TABLE dbo.[establishment]
    ADD CONSTRAINT DF_establishment_dateLastUpdated DEFAULT GETUTCDATE() FOR dateLastUpdated
ALTER TABLE dbo.[response]
    ADD CONSTRAINT DF_response_dateLastUpdated DEFAULT GETUTCDATE() FOR dateLastUpdated
ALTER TABLE dbo.[submission]
    ADD CONSTRAINT DF_submission_dateLastUpdated DEFAULT GETUTCDATE() FOR dateLastUpdated

-- Make all dateUpdated columns non-nullable

ALTER TABLE dbo.[user]
    ALTER COLUMN dateLastUpdated DATETIME NOT NULL
ALTER TABLE dbo.[establishment]
    ALTER COLUMN dateLastUpdated DATETIME NOT NULL
ALTER TABLE dbo.[response]
    ALTER COLUMN dateLastUpdated DATETIME NOT NULL
ALTER TABLE dbo.[submission]
    ALTER COLUMN dateLastUpdated DATETIME NOT NULL

GO

-- Add triggers that update the dateUpdated columns automatically

CREATE TRIGGER dbo.tr_user
    ON dbo.[user]
    FOR INSERT, UPDATE
    AS
BEGIN
    UPDATE U
    SET dateLastUpdated = GETUTCDATE()
    FROM dbo.[user] U
    JOIN inserted I ON I.id = U.id
END

GO

CREATE TRIGGER dbo.tr_establishment
    ON dbo.[establishment]
    FOR INSERT, UPDATE
    AS
BEGIN
    UPDATE E
    SET dateLastUpdated = GETUTCDATE()
    FROM dbo.[establishment] E
    JOIN inserted I ON I.id = E.id
END

GO

CREATE TRIGGER dbo.tr_response
    ON dbo.[response]
    FOR INSERT, UPDATE
    AS
BEGIN
    DECLARE @now DATETIME = GETUTCDATE()

    UPDATE R
    SET dateLastUpdated = @now
    FROM dbo.[response] R
    JOIN inserted I ON I.id = R.id

    -- responses should update the lastUpdated time of the parent submission
    UPDATE S
    SET dateLastUpdated = @now
    FROM dbo.submission S
    JOIN inserted I ON I.submissionId = S.id
END

GO

CREATE TRIGGER dbo.tr_submission
    ON dbo.[submission]
    FOR INSERT, UPDATE
    AS
BEGIN
    UPDATE S
    SET dateLastUpdated = GETUTCDATE()
    FROM dbo.[submission] S
    JOIN inserted I ON I.id = S.id
END

GO

-- Simplify GetSectionStatusesProc with the new columns

ALTER PROCEDURE dbo.GetSectionStatuses @categoryId NVARCHAR(30), @establishmentId INT
AS

SELECT
    CurrentSubmission.sectionId,
    CurrentSubmission.completed,
    LastCompleteSubmission.maturity AS lastMaturity,
    CurrentSubmission.dateCreated,
    CurrentSubmission.dateLastUpdated
FROM Contentful.Sections CS
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
          CS.Id = S.sectionId
      AND S.establishmentId = @establishmentId
      AND S.deleted = 0
    ORDER BY S.dateCreated DESC
) CurrentSubmission
-- Use maturity from most recent complete submission (if there is one) so that user always sees recommendation
OUTER APPLY (
    SELECT TOP 1
        maturity
    FROM [dbo].submission S
    WHERE
          CS.Id = S.sectionId
      AND S.establishmentId = @establishmentId
      AND S.deleted = 0
      AND s.completed = 1
    ORDER BY S.dateCreated DESC
) LastCompleteSubmission
WHERE
    CS.CategoryId = @categoryId
