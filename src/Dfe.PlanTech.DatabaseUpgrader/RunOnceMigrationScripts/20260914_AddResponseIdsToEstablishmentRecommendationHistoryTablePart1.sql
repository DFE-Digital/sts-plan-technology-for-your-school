-- Runtime approx. 10 mins

BEGIN TRAN;

-- Add new column to estRecHist to store notes makde by developers
-- and the system.
IF NOT EXISTS (
	SELECT
		*
	FROM
		INFORMATION_SCHEMA.COLUMNS
	WHERE
		TABLE_SCHEMA = 'dbo'
		AND TABLE_NAME = 'establishmentRecommendationHistory'
		AND COLUMN_NAME = 'systemNote'
)
BEGIN
  ALTER TABLE dbo.establishmentRecommendationHistory
  ADD systemNote VARCHAR(50) NULL;
END

-- Add a new schema to the database to hold temporary data
-- which we can use as a 'scratchpad', for working.
IF NOT EXISTS
(
  SELECT
    *
  FROM
	  sys.schemas ss
  WHERE
	  ss.name = 'temp'
)
BEGIN
    EXEC('CREATE SCHEMA temp');
END
;

-- Build new estRecHist data in the same way as we did for
-- the November migration.

-- Clear out existing data
DROP TABLE IF EXISTS temp.submissionResponseRecommendations;
DROP TABLE IF EXISTS temp.newErhData;

-- Build a lookup of users' establishments
DROP TABLE IF EXISTS temp.userEstablishments;

SELECT DISTINCT
	si.userId,
	e.id AS establishmentId,
	e.establishmentRef,
	e.establishmentType,
	e.orgName,
	e.dateCreated,
	e.dateLastUpdated,
	e.groupUid,
	eg.groupType
INTO
  temp.userEstablishments
FROM
	dbo.[signIn] si
	JOIN dbo.[establishment] e ON e.id = si.establishmentId
	LEFT JOIN dbo.establishmentGroup eg ON eg.uid = e.groupUid
WHERE
	eg.groupType IS NULL
  OR eg.groupType != 'Multi-academy trust'
;

-- Find the most recent responses and their corresponding recommendations
DROP TABLE IF EXISTS temp.mostRecentResponses;

SELECT DISTINCT
	r.id AS responseId,
  r.dateCreated,
  r.userId,
  r.userEstablishmentId,
	r.submissionId,
	r.questionId,
	r.answerId,
	r.dateLastUpdated,
	ROW_NUMBER() OVER (
			PARTITION BY submissionId, questionId
			ORDER BY id DESC
	) AS rowNumber
INTO
  temp.mostRecentResponses
FROM
	dbo.response r
;

-- Begin with the 'simple' mapping cases...
DROP TABLE IF EXISTS temp.responseRecommendations;

SELECT
	r.responseId,
	r.userId,
	r.submissionId,
	r.questionId,
	r.answerId,
	r.dateLastUpdated,
	a.contentfulRef AS answerRef,
	ras.recommendationRef,
	ras.recommendationStatus
INTO
  temp.responseRecommendations
FROM
	dbo.answer a
	JOIN temp.mostRecentResponses r ON r.answerId = a.id AND r.rowNumber = 1
	OUTER APPLY
	(
		SELECT DISTINCT
			ras.answerRef,
			ras.recommendationRef,
			ras.recommendationStatus
		FROM
			migration.recommendationAnswerStatus ras
		WHERE
			ras.answerRef = a.contentfulRef
	) ras
;

-- ...then add the special cases.
DROP TABLE IF EXISTS temp.specialCaseRuleConfig;

SELECT
  *
INTO
  temp.specialCaseRuleConfig
FROM
(
  VALUES
	-- === ROLES AND RESPONSIBILITIES === (Q4 & Q5)
	-- previousAnswerRef,       followingAnswerRef,       recommendationRef,        recommendationStatus
    ('6wtaOvArqw58k6dJpu6fIs', '3ph9hLatVB7LQZRmyVO3t8', '6VokhhomxQO0zNIIW0yCMV', 'Complete'),
    ('6wtaOvArqw58k6dJpu6fIs', '4B6KINKgL1Ue8juzqk1b3w', '6VokhhomxQO0zNIIW0yCMV', 'InProgress'),
    ('6wtaOvArqw58k6dJpu6fIs', '4HmuemJ2GYJ5P4jRo00HMl', '6VokhhomxQO0zNIIW0yCMV', 'InProgress'),
    ('6wtaOvArqw58k6dJpu6fIs', '4rHUnpmjbbIqQ5IN0qq237', '6VokhhomxQO0zNIIW0yCMV', 'InProgress'),

  -- === NETWORK SWITCHING: Q3 & Q4 ===
	-- previousAnswerRef,       followingAnswerRef,       recommendationRef,       recommendationStatus
    ('66D2bAEguN7DLy2lE0vWIE', 'F2LO3TVCruNR0JoLARvgl',  '8Fs5H36EUt7pj7GOzOx3t', 'Complete'),
    ('66D2bAEguN7DLy2lE0vWIE', '7jSYMmNdUZIyY2qXaqxPjn', '8Fs5H36EUt7pj7GOzOx3t', 'InProgress'),
    ('66D2bAEguN7DLy2lE0vWIE', '5LWGaH21US4H3AWHaPG72c', '8Fs5H36EUt7pj7GOzOx3t', 'InProgress'),

  -- === NETWORK SWITCHING: Q6 & Q7, A1 = Yes ===
	-- previousAnswerRef,       followingAnswerRef,       recommendationRef,        recommendationStatus
    ('7fbPOsBczKXuaR3KWoXKcX', '2LDzkUVH0UoCUaLrYNyn02', '79lhdAJX6Hm02j5ExIIHdr', 'Complete'),
    ('7fbPOsBczKXuaR3KWoXKcX', '3mAPnOPseY9LohrelHAuQS', '79lhdAJX6Hm02j5ExIIHdr', 'InProgress'),
    ('7fbPOsBczKXuaR3KWoXKcX', '6zSzWhsxgYw1x9wM7I4EAA', '79lhdAJX6Hm02j5ExIIHdr', 'InProgress'),
    ('7fbPOsBczKXuaR3KWoXKcX', '75Uj6acR1Uyyh2E8SAxJDs', '79lhdAJX6Hm02j5ExIIHdr', 'InProgress'),

  -- === NETWORK SWITCHING: Q6 & Q7, A1 = No ===
	-- previousAnswerRef,       followingAnswerRef,       recommendationRef,        recommendationStatus
    ('4A4C0ADw0C4yV72wcmLrDY', '2LDzkUVH0UoCUaLrYNyn02', '79lhdAJX6Hm02j5ExIIHdr', 'InProgress'),
    ('4A4C0ADw0C4yV72wcmLrDY', '3mAPnOPseY9LohrelHAuQS', '79lhdAJX6Hm02j5ExIIHdr', 'NotStarted'),
    ('4A4C0ADw0C4yV72wcmLrDY', '6zSzWhsxgYw1x9wM7I4EAA', '79lhdAJX6Hm02j5ExIIHdr', 'NotStarted'),
    ('4A4C0ADw0C4yV72wcmLrDY', '75Uj6acR1Uyyh2E8SAxJDs', '79lhdAJX6Hm02j5ExIIHdr', 'NotStarted'),

  -- === NETWORK SWITCHING: Q6 & Q7, A1 = Not sure ===
  -- previousAnswerRef,      followingAnswerRef,       recommendationRef,        recommendationStatus
    ('AjK1OS7lcRONfLyDggUE8', '2LDzkUVH0UoCUaLrYNyn02', '79lhdAJX6Hm02j5ExIIHdr', 'InProgress'),
    ('AjK1OS7lcRONfLyDggUE8', '3mAPnOPseY9LohrelHAuQS', '79lhdAJX6Hm02j5ExIIHdr', 'NotStarted'),
    ('AjK1OS7lcRONfLyDggUE8', '6zSzWhsxgYw1x9wM7I4EAA', '79lhdAJX6Hm02j5ExIIHdr', 'NotStarted'),
    ('AjK1OS7lcRONfLyDggUE8', '75Uj6acR1Uyyh2E8SAxJDs', '79lhdAJX6Hm02j5ExIIHdr', 'NotStarted'),

  -- === WIRELESS: Q6 & Q7, A1 = Yes ===
	-- previousAnswerRef,      followingAnswerRef,        recommendationRef,        recommendationStatus
    ('7viObsilpXGsDIhtjhQSlT', 'nj7dD3jxf6ljGR3an2lft',  '2ZVBbsVJXLzQyqFHfjAtF1', 'Complete'),
    ('7viObsilpXGsDIhtjhQSlT', '4TrRGa12fNsG8dWC6Kfcrw', '2ZVBbsVJXLzQyqFHfjAtF1', 'InProgress'),
    ('7viObsilpXGsDIhtjhQSlT', '21rMdLj36MijZhqBW5Xkhw', '2ZVBbsVJXLzQyqFHfjAtF1', 'InProgress')
) scrc(previousAnswerRef, followingAnswerRef, recommendationRef, recommendationStatus)
;

DROP TABLE IF EXISTS temp.specialCaseResponseAnswers;

SELECT
  r.responseId,
  r.userId,
  r.submissionId,
  r.questionId,
  r.answerId,
  r.dateLastUpdated,
  a.contentfulRef AS answerRef
INTO
  temp.specialCaseResponseAnswers
FROM
  dbo.answer a
  JOIN temp.mostRecentResponses r
    ON r.answerId = a.id
    AND r.rowNumber = 1
WHERE
  a.contentfulRef IN
  (
    SELECT DISTINCT previousAnswerRef FROM temp.specialCaseRuleConfig
    UNION
    SELECT DISTINCT followingAnswerRef FROM temp.specialCaseRuleConfig
  )
;

INSERT INTO
  temp.responseRecommendations
SELECT
  rr1.responseId,
  rr1.userId,
  rr1.submissionId,
  rr1.questionId,
  rr1.answerId,
  rr1.dateLastUpdated,
  rr2.answerRef,
  rc.recommendationRef,
  rc.recommendationStatus
FROM
  temp.specialCaseResponseAnswers rr1
  JOIN temp.specialCaseResponseAnswers rr2
    ON rr1.userId = rr2.userId
    AND rr1.submissionId = rr2.submissionId
    AND rr1.answerRef <> rr2.answerRef
  JOIN temp.specialCaseRuleConfig rc
    ON rc.previousAnswerRef = rr1.answerRef
    AND rc.followingAnswerRef = rr2.answerRef
;

-- Find the corresponding recommendations for each response
SELECT DISTINCT
	s.id AS submissionId,
	s.establishmentId,
	s.dateCompleted AS dateSubmissionCompleted,
	r.userId,
	sr.sectionRef,
	rr.responseId,
	rr.dateLastUpdated,
	rr.answerId,
	rr.answerRef,
	rec.id AS recommendationId,
	sr.recommendationRef,
	ISNULL(rr.recommendationStatus, 'NotStarted') AS recommendationStatus
INTO
  temp.submissionResponseRecommendations
FROM
	dbo.submission s
	JOIN migration.sectionRecommendation sr ON sr.sectionRef = s.sectionId
	JOIN dbo.response r ON r.submissionId = s.id
	LEFT JOIN temp.responseRecommendations rr ON rr.submissionId = s.id AND rr.recommendationRef = sr.recommendationRef
	LEFT JOIN dbo.recommendation rec ON rec.contentfulRef = sr.recommendationRef
WHERE
	s.status = 'CompleteReviewed'
;

COMMIT TRAN;


BEGIN TRAN;

DECLARE @systemNote NVARCHAR(50) = FORMAT(SYSUTCDATETIME(), 'yyyy-MM-dd HH:mm:ss') + ': Add responseId';

-- Build a table of the updated data to be merged into
-- establishmentRecommendationHistory.
SELECT
	erh.id AS erhId,
	s.dateSubmissionCompleted,
	ue.establishmentId,
	NULL AS matEstablishmentId,
	s.recommendationId,
	s.userId,
	NULL AS previousStatus,
	s.recommendationStatus AS newStatus,
	NULL AS noteText,
	s.responseId,
	@systemNote AS systemNote,
	ROW_NUMBER() OVER (
        PARTITION BY erh.Id
        ORDER BY s.dateSubmissionCompleted DESC
	) AS rowNumber
INTO
	temp.newErhData
FROM
	temp.submissionResponseRecommendations s
	JOIN temp.userEstablishments ue ON
		ue.establishmentId = s.establishmentId
		AND ue.userId = s.userId
	JOIN dbo.establishmentRecommendationHistory erh ON
		erh.establishmentId = ue.establishmentId
		AND erh.matEstablishmentId IS NULL
		AND erh.recommendationId = s.recommendationId
		AND erh.userId = s.userId
		AND erh.previousStatus IS NULL
		AND erh.newStatus = s.recommendationStatus
		AND erh.noteText IS NULL
		AND erh.responseId IS NULL
;

-- Check the counts
SELECT
  COUNT(*) AS existingRows
FROM
  dbo.establishmentRecommendationHistory erh

SELECT
  COUNT(*) AS newDataRows
FROM
  temp.newErhData newData

SELECT
  COUNT(*) AS matchedRows
FROM
  temp.newErhData newData
  JOIN dbo.establishmentRecommendationHistory erh ON erh.id = newData.erhId

COMMIT TRAN;


BEGIN TRAN;

SELECT
  COUNT(*) AS recordsWithNoReponseIdBeforeUpdate
FROM
	dbo.establishmentRecommendationHistory erh
WHERE
  erh.responseId IS NULL
;

UPDATE
	erh
SET
	erh.responseId = newData.responseId,
	erh.systemNote = newData.systemNote
FROM
	dbo.establishmentRecommendationHistory erh
	JOIN temp.newErhData newData ON newData.erhId = erh.id
WHERE
	newData.rowNumber = 1
  AND newData.responseId IS NOT NULL
;

SELECT
  COUNT(*) AS recordsWithNoReponseIdAfterUpdate
FROM
	dbo.establishmentRecommendationHistory erh
WHERE
  erh.responseId IS NULL
;

COMMIT TRAN;


BEGIN TRAN;

-- Remove duplicate rows
SELECT
	erh.*,
	ROW_NUMBER() OVER (
		PARTITION BY
			erh.dateCreated,
			erh.establishmentId,
			erh.recommendationId,
			erh.userId,
			erh.newStatus,
			erh.responseId,
			erh.systemNote
		ORDER BY erh.id DESC
	) AS rowNumber
INTO
	#numberedErh
FROM
	dbo.establishmentRecommendationHistory erh
;

SELECT COUNT(*) AS recordsBefore FROM dbo.establishmentRecommendationHistory;

DELETE
FROM
	dbo.establishmentRecommendationHistory
WHERE
	id IN (SELECT id FROM #numberedErh ne WHERE ne.rowNumber > 1)
;

SELECT COUNT(*) AS recordsAfter FROM dbo.establishmentRecommendationHistory;

COMMIT TRAN;
