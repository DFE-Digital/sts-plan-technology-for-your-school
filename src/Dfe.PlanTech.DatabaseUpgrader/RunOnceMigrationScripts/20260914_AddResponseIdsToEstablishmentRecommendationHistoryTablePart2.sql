-- There are still 74,779 records with no responseId
SELECT
  COUNT(*) AS erhRecordsWithNoResponseId
FROM
  dbo.establishmentRecommendationHistory erh
WHERE
  erh.responseId IS NULL
;

-- Now we try mapping a different way. Rather than going via
-- Contentful, we try to find which records we can update
-- simply by finding the ones with a definitive link across
-- the database.

DROP TABLE IF EXISTS temp.submissionResponseQuestions;

SELECT
	s.sectionId,
	s.sectionName,
	s.id AS submissionId,
	s.dateCompleted AS dateSubmissionCompleted,
	r.responseId,
	r.dateCreated AS responseDate,
	r.userEstablishmentId,
	s.establishmentId,
	r.userId,
	r.questionId,
	q.contentfulRef AS questionRef,
	q.questionText
INTO
	temp.submissionResponseQuestions
FROM
	dbo.establishment e
	JOIN dbo.submission s ON s.establishmentId = e.id
	JOIN temp.mostRecentResponses r ON
		r.submissionId = s.id
		AND r.rowNumber = 1
	JOIN dbo.question q ON q.id = r.questionId
;

-- 151,770 records
SELECT
	COUNT(*) AS responseDataCount
FROM
	temp.submissionResponseQuestions rd
;

-- Now attempt to find which recommendation goes with which question
DROP TABLE IF EXISTS temp.questionRecommendationMap;

WITH questionSections AS
(
	SELECT DISTINCT
		s.sectionId,
		q.id AS questionId,
		q.[order],
		q.questionText
	FROM
		dbo.submission s
		JOIN dbo.response r ON r.submissionId = s.id
		JOIN dbo.question q ON q.id = r.questionId
)

SELECT DISTINCT
	msq.sectionRef,
	qs.questionId,
	mq.contentfulRef AS questionRef,
	qs.[order],
	qs.questionText,
	rec.id AS recommendationId
INTO
	temp.questionRecommendationMap
FROM
	questionSections qs
	JOIN migration.sectionQuestion msq ON msq.sectionRef = qs.sectionId
	JOIN migration.question mq ON
		mq.contentfulRef = msq.questionRef
		AND mq.internalName LIKE '%question ' + CAST(qs.[order] AS NVARCHAR(50)) + '%'
	JOIN dbo.recommendation rec ON rec.questionContentfulRef = mq.contentfulRef
ORDER BY
	msq.sectionRef,
	qs.[order]
;

DROP TABLE IF EXISTS temp.responseErhMap;

SELECT
	trd.submissionId,
	trd.dateSubmissionCompleted,
	trd.userEstablishmentId,
	trd.establishmentId,
	trd.userId,
	trd.responseId,
	trd.responseDate,
	trd.questionRef,
	trd.questionText,
	rec.id AS recommendationId,
	erh.id AS erhId,
	erh.dateCreated,
	erh.previousStatus,
	erh.newStatus,
	erh.noteText
INTO
	temp.responseErhMap
FROM
	temp.submissionResponseQuestions trd
	JOIN temp.questionRecommendationMap tqrm ON tqrm.questionId = trd.questionId
	JOIN dbo.recommendation rec ON rec.id = tqrm.recommendationId
	JOIN dbo.establishmentRecommendationHistory erh ON
		erh.establishmentId = trd.establishmentId
    AND (erh.matEstablishmentId IS NULL OR (erh.matEstablishmentId IS NOT NULL AND erh.matEstablishmentId = trd.userEstablishmentId))
    AND erh.recommendationId = rec.id
    AND erh.userId = trd.userId
    AND erh.previousStatus IS NULL
    AND erh.noteText IS NULL
		AND erh.dateCreated > trd.responseDate
    AND erh.responseId IS NULL
;

-- 67,884 records
SELECT
	COUNT(*) AS responseErhMapCount
FROM
	temp.responseErhMap map
  JOIN dbo.establishmentRecommendationHistory erh ON erh.id = map.erhId
WHERE
	map.responseId IS NOT NULL
  AND erh.responseId IS NULL
;

-- 5,912 ERH records have no matching map
DROP TABLE IF EXISTS temp.unmatchedErhIds;

SELECT
	erh.id
INTO
	temp.unmatchedErhIds
FROM
	dbo.establishmentRecommendationHistory erh
	LEFT JOIN temp.responseErhMap map ON map.erhId = erh.id
WHERE
	erh.responseId IS NULL
	AND erh.previousStatus IS NULL
	AND map.erhId IS NULL
;

SELECT COUNT(*) FROM temp.unmatchedErhIds;

-- Of those, 3,625 records match a submission on establishmentId and date,
-- without duplication
DROP TABLE IF EXISTS #matchedOnEstablishmentIdAndDate;

SELECT
	erh.id AS erhId,
	s.id AS submissionId
INTO
	#matchedOnEstablishmentIdAndDate
FROM
	temp.unmatchedErhIds um
	JOIN dbo.establishmentRecommendationHistory erh ON um.id = erh.id
	JOIN dbo.submission s ON
		s.establishmentId = erh.establishmentId
		AND s.dateCompleted = erh.dateCreated
;

SELECT COUNT(*) FROM #matchedOnEstablishmentIdAndDate;

SELECT
	m.erhId,
	COUNT(m.submissionId) AS submissionCount
FROM
	#matchedOnEstablishmentIdAndDate m
GROUP BY
	m.erhId
HAVING
	COUNT(m.submissionId) > 1
;

-- That leaves 2,287
DROP TABLE IF EXISTS #unmatchedOnEstablishmentIdAndDate;

SELECT
	erh.id
INTO
	#unmatchedOnEstablishmentIdAndDate
FROM
	temp.unmatchedErhIds um
	JOIN dbo.establishmentRecommendationHistory erh ON um.id = erh.id
	LEFT JOIN dbo.submission s ON
		s.establishmentId = erh.establishmentId
		AND s.dateCompleted = erh.dateCreated
WHERE
	s.id IS NULL
;

SELECT COUNT(*) FROM #unmatchedOnEstablishmentIdAndDate;

-- With a bit of experimentation, we find that of the remaining 2,329
-- we can match 2,257 1:1 with a submission by matching within 12 seconds
-- of the submission date. The remaining 30 are a lost cause.
DROP TABLE IF EXISTS #matchedOnEstablishmentIdAndWithin12SecondsOfDate;

SELECT
	erh.id AS erhId,
	s.id AS submissionId
INTO
	#matchedOnEstablishmentIdAndWithin12SecondsOfDate
FROM
	#unmatchedOnEstablishmentIdAndDate um
	JOIN dbo.establishmentRecommendationHistory erh ON um.id = erh.id
	JOIN dbo.submission s ON
		s.establishmentId = erh.establishmentId
		AND s.dateCompleted BETWEEN DATEADD(SECOND, -1, erh.dateCreated) AND DATEADD(SECOND, 12, erh.dateCreated)
;

SELECT COUNT(*) FROM #matchedOnEstablishmentIdAndWithin12SecondsOfDate;

SELECT
	m.erhId,
	COUNT(m.submissionId) AS submissionCount
FROM
	#matchedOnEstablishmentIdAndDate m
GROUP BY
	m.erhId
HAVING
	COUNT(m.submissionId) > 1
;

-- Now, let's see whether we can find a 1:1 map for any of those.
DROP TABLE IF EXISTS temp.additionalErhResponseMaps;

SELECT
	erh.id AS erhId,
	r.id AS responseId
INTO
  temp.additionalErhResponseMaps
FROM
	(
		SELECT erhId, submissionId FROM #matchedOnEstablishmentIdAndDate
		UNION
		SELECT erhId, submissionId FROM #matchedOnEstablishmentIdAndWithin12SecondsOfDate
	) matches
	JOIN dbo.establishmentRecommendationHistory erh ON erh.id = matches.erhId
	JOIN dbo.recommendation rec ON rec.id = erh.recommendationId
	JOIN dbo.submission s ON s.id = matches.submissionId
	JOIN dbo.response r ON
		r.submissionId = s.id
		AND r.userId = erh.userId
WHERE
  erh.responseId IS NULL
;

WITH singleResponseCounts AS
(
	SELECT
		m.erhId,
		COUNT(m.responseId) AS responseCount
	FROM
		temp.additionalErhResponseMaps m
	GROUP BY
		m.erhId
	HAVING
		COUNT(m.responseId) = 1
)

SELECT
	am.erhId,
	am.responseId
FROM
	temp.additionalErhResponseMaps am
	JOIN singleResponseCounts sc ON sc.erhId = am.erhId
;

-- By pure luck, we can do it for 1,097.
SELECT COUNT(*) FROM temp.additionalErhResponseMaps;

-- So we're left with 93,568 which have some kind of response somewhere
DROP TABLE IF EXISTS temp.additionalMaps;

WITH allMaps AS
(
	SELECT
		map1.erhId,
		map1.responseId
	FROM
		temp.responseErhMap map1 -- The ones

	UNION

	SELECT
		map2.erhId,
		map2.responseId
	FROM
		temp.additionalErhResponseMaps map2
)

SELECT
	erhId,
	responseId
INTO
  temp.additionalMaps
FROM
	allMaps
;

-- 53,401 rows have a single response.
-- 11,952 have more than one and so are unusable.
SELECT
  erh.id,
  COUNT(DISTINCT tam.responseId) AS responseCount
FROM
  dbo.establishmentRecommendationHistory erh
  JOIN temp.additionalMaps tam ON tam.erhId = erh.id
GROUP BY
  erh.id
HAVING
  COUNT(DISTINCT tam.responseId) = 1 -- Change to '> 1' to see unusable rows
ORDER BY
  COUNT(DISTINCT tam.responseId) DESC
;

-- Store the ones with a single response
DROP TABLE IF EXISTS temp.newErhDataByAlternativeMethod;

WITH additionalMapsWithSingleResponse AS
(
  SELECT
    erh.id AS erhId
  FROM
    dbo.establishmentRecommendationHistory erh
    JOIN temp.additionalMaps tam ON tam.erhId = erh.id
  GROUP BY
    erh.id
  HAVING
    COUNT(DISTINCT tam.responseId) = 1
)

SELECT
  erh.id,
  erh.responseId AS originalResponseId,
  tam.erhId,
  tam.responseId AS newResponseId
INTO
  temp.newErhDataByAlternativeMethod
FROM
  dbo.establishmentRecommendationHistory erh
  JOIN additionalMapsWithSingleResponse sr ON sr.erhId = erh.id
  JOIN temp.additionalMaps tam ON tam.erhId = erh.id
WHERE
  tam.responseId IS NOT NULL
;

DROP TABLE IF EXISTS temp.possibleAdditionalMatches;

SELECT
  erh.id AS erhId,
  method2.newResponseId
INTO
  temp.possibleAdditionalMatches
FROM
  dbo.establishmentRecommendationHistory erh
  LEFT JOIN temp.newErhData method1 ON method1.erhId = erh.id
  JOIN temp.newErhDataByAlternativeMethod method2 ON method2.erhId = erh.id
WHERE
  erh.responseId IS NULL
  AND method1.erhId IS NULL
  OR method1.responseId IS NULL
;

SELECT COUNT(*) FROM temp.possibleAdditionalMatches;

-- Are there any mismatches?
SELECT
  erh.systemNote,
  erh.dateCreated AS erhDate,
  s.dateCompleted AS submissionDate,
  erh.userId AS erhUserId,
  r.userId AS responseUserId,
  erh.establishmentId AS erhEstablishmentId,
  s.establishmentId AS submissionEstablishmentId,
  erh.matEstablishmentId AS erhMatEstablishmentId,
  r.userEstablishmentId,
  s.sectionName,
  ms.[name] AS contentfulName
FROM
  dbo.establishmentRecommendationHistory erh
  JOIN temp.possibleAdditionalMatches pam ON pam.erhId = erh.id
  JOIN dbo.response r ON r.id = pam.newResponseId
  JOIN dbo.question q ON q.id = r.questionId
  JOIN dbo.submission s ON s.id = r.submissionId
  LEFT JOIN migration.sectionQuestion msq ON msq.sectionRef = s.sectionId AND msq.questionRef = q.contentfulRef
  LEFT JOIN migration.section ms ON ms.contentfulRef = msq.sectionRef
WHERE
  erh.userId != r.userId
  OR erh.establishmentId != s.establishmentId
  OR (erh.matEstablishmentId IS NOT NULL AND erh.matEstablishmentId != r.userEstablishmentId)
  OR s.sectionName != ms.[name]
;

-- No! So have we covered off many of the unmatched ones?
SELECT
  erh.systemNote,
  erh.dateCreated AS erhDate,
  s.dateCompleted AS submissionDate,
  erh.userId AS erhUserId,
  r.userId AS responseUserId,
  erh.establishmentId AS erhEstablishmentId,
  s.establishmentId AS submissionEstablishmentId,
  erh.matEstablishmentId AS erhMatEstablishmentId,
  r.userEstablishmentId,
  s.sectionName,
  ms.[name] AS contentfulName
FROM
  temp.unmatchedErhIds tuerh
  JOIN dbo.establishmentRecommendationHistory erh ON erh.id = tuerh.id
  JOIN temp.possibleAdditionalMatches pam ON pam.erhId = erh.id
  JOIN dbo.response r ON r.id = pam.newResponseId
  JOIN dbo.question q ON q.id = r.questionId
  JOIN dbo.submission s ON s.id = r.submissionId
  LEFT JOIN migration.sectionQuestion msq ON msq.sectionRef = s.sectionId AND msq.questionRef = q.contentfulRef
  LEFT JOIN migration.section ms ON ms.contentfulRef = msq.sectionRef
WHERE
  erh.responseId IS NULL
;

-- Yes. 1,097, as we saw in singleResponseCounts.
-- So now we update the additional ones we found.

-- We have 53,404 additional matches to process.
SELECT
  COUNT(*)
FROM
  temp.possibleAdditionalMatches newData
  JOIN dbo.establishmentRecommendationHistory erh ON newData.erhId = erh.id
WHERE
  erh.responseId IS NULL
  AND newData.newResponseId IS NOT NULL
;

-- There are 74,779 before
SELECT
  COUNT(*) AS recordsWithNoReponseIdBeforeUpdate
FROM
	dbo.establishmentRecommendationHistory erh
WHERE
  erh.responseId IS NULL
;

DECLARE @systemNote NVARCHAR(50) = FORMAT(SYSUTCDATETIME(), 'yyyy-MM-dd HH:mm:ss') + ': Add responseId';

UPDATE
	erh
SET
	erh.responseId = newData.newResponseId,
	erh.systemNote = @systemNote
FROM
	dbo.establishmentRecommendationHistory erh
	JOIN temp.possibleAdditionalMatches newData ON newData.erhId = erh.id
WHERE
  erh.responseId IS NULL
  AND newData.newResponseId IS NOT NULL
;

-- There are 21,378 after
SELECT
  COUNT(*) AS recordsWithNoReponseIdAfterUpdate
FROM
	dbo.establishmentRecommendationHistory erh
WHERE
  erh.responseId IS NULL
;
