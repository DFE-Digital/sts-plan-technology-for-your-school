/*
HOW TO USE THIS SCRIPT
======================

- Add "--" to the "/ *" under block 1 to activate it (line 27)
- Run block 1 to generate submission data and user actions
- Confirm data looks reasonable
  - tempDataCount should be ≤ realDataCount (we ignore submissions with no responses)
  - creation/updateUserActionCount should equal tempDataCount
  - completionUserActionCount will most likely be less if there are many submissions in progres
  - MATs and SATs should not have any missing MAT establishment IDs. Schools are expected to have lots missing.
- Remove the "--" you added from the start of block 1

- Add "--" to the "/ *" under block 2 to activate it (line 276)
- Run block 2 to add user actions to the database and associate them with submissions
- Confirm the associated establishment names look right
- Change "ROLLBACK TRAN" at the end of block 2 to "COMMIT TRAN";
- Re-run block 2
- Change "COMMIT TRAN" to "ROLLBACK TRAN";
- Remove the "--" you added from the start of block 2

- Add "--" to the "/ *" under block 3 to activate it (line 389)
- Clean up temporary tables
- Remove the "--" you added from the start of block 3
*/

/*******************************************************************************
* BLOCK 1: Build submission lookup data and generate user actions              *
*******************************************************************************/

/*
-- Drop the #submissionData temporary table if it exists
IF OBJECT_ID(N'tempdb..#submissionData', N'U') IS NOT NULL
  DROP TABLE #submissionData;

-- Build the lookup data we will use to generate the user actions
WITH responsesRanked AS
(
	SELECT
		s.id AS submissionId,
		s.dateCreated,
		s.dateLastUpdated,
		s.dateCompleted,
		s.status,
		s.establishmentId,
		r.id AS responseId,
		r.userId,
		r.userEstablishmentId,
		CASE WHEN r.userEstablishmentId <> s.establishmentId THEN 1 ELSE 0 END AS isMatUser,
		r.dateCreated AS responseDate,
		ROW_NUMBER() OVER (PARTITION BY s.id ORDER BY r.dateCreated, r.id) AS rowAsc,
		ROW_NUMBER() OVER (PARTITION BY s.id ORDER BY r.dateCreated DESC, r.id DESC) AS rowDesc
	FROM
		dbo.submission s
		INNER JOIN dbo.establishment e ON e.id = s.establishmentId
		INNER JOIN dbo.response r ON r.submissionId = s.id
		OUTER APPLY (
			SELECT COUNT(el.id) AS schoolCount
			FROM dbo.establishmentLink el
			WHERE el.groupUid = e.groupUid
		) links
),

submissionInteractors AS
(
	SELECT
		rr.submissionId,
		COUNT(rr.responseId) AS responseCount,
		COUNT(DISTINCT rr.userId) AS userCount,
		MAX(CASE WHEN rr.rowAsc = 1 THEN rr.responseDate END) AS firstResponseTime,
		MAX(CASE WHEN rr.rowAsc = 1 THEN rr.userId END) AS firstResponderId,
		MAX(CASE WHEN rr.rowAsc = 1 AND rr.isMatUser = 1 THEN rr.userEstablishmentId ELSE NULL END) AS firstResponderMatEstablishmentId,
		MAX(CASE WHEN rr.rowDesc = 1 THEN rr.responseDate END) AS lastResponseTime,
		MAX(CASE WHEN rr.rowDesc = 1 THEN rr.userId END) AS lastResponderId,
		MAX(CASE WHEN rr.rowDesc = 1 AND rr.isMatUser = 1 THEN rr.userEstablishmentId ELSE NULL END) AS lastResponderMatEstablishmentId
	FROM
		responsesRanked rr
	GROUP BY
		rr.submissionId
)


SELECT
	si.submissionId,
	sd.dateCreated,
	sd.dateLastUpdated,
	sd.dateCompleted,
	sd.status,
	si.responseCount,
	si.userCount,
	si.firstResponseTime,
	si.firstResponderId,
	si.lastResponseTime,
	si.lastResponderId,
	sd.establishmentId,
	si.firstResponderMatEstablishmentId AS createdMatEstablishmentId,
	si.lastResponderMatEstablishmentId AS updatedMatEstablishmentId
INTO
	#submissionData
FROM
	submissionInteractors si
	INNER JOIN
	(
		SELECT DISTINCT
			rr.submissionId,
			rr.dateCreated,
			rr.dateLastUpdated,
			rr.dateCompleted,
			rr.status,
			rr.establishmentId
		FROM
			responsesRanked rr
	) sd ON sd.submissionId = si.submissionId

SELECT COUNT(*) AS tempDataCount FROM #submissionData;
SELECT COUNT(*) AS realDataCount FROM dbo.submission;

-- Drop the #userActionData temporary table if it exists
IF OBJECT_ID(N'tempdb..#userActionData', N'U') IS NOT NULL
  DROP TABLE #userActionData;

-- Create a table to store the new user actions
CREATE TABLE #userActionData
(
	submissionId INT,
	actionType NVARCHAR(50),
	userActionId UNIQUEIDENTIFIER,
	userId INT,
	establishmentId INT,
	matEstablishmentId INT,
	requestedUrl NVARCHAR(4000),
	dateCreated DATETIME2
);

-- Declare magic strings to identify row types
DECLARE
	@creationUserActionCount INT,
	@updateUserActionCount INT,
	@completionUserActionCount INT

-- Generate creation actions
INSERT INTO #userActionData
(
	submissionId,
	actionType,
	userActionId,
	userId,
	establishmentId,
	matEstablishmentId,
	requestedUrl,
	dateCreated
)
SELECT
	sd.submissionId,
	'Creation',
	NEWID(),
	sd.firstResponderId,
	sd.establishmentId,
	sd.createdMatEstablishmentId,
	'submissionId=' + CAST(sd.submissionId AS NVARCHAR(30)),
	sd.dateCreated
FROM
	#submissionData sd;

SELECT @creationUserActionCount = COUNT(*) FROM #userActionData;

-- Generate last-updated actions
INSERT INTO #userActionData
(
	submissionId,
	actionType,
	userActionId,
	userId,
	establishmentId,
	matEstablishmentId,
	requestedUrl,
	dateCreated
)
SELECT
	sd.submissionId,
	'Update',
	NEWID(),
	sd.lastResponderId,
	sd.establishmentId,
	sd.updatedMatEstablishmentId,
	'submissionId=' + CAST(sd.submissionId AS NVARCHAR(30)),
	sd.dateLastUpdated
FROM
	#submissionData sd;

SELECT @updateUserActionCount = COUNT(*) - @creationUserActionCount FROM #userActionData;

-- Generate completion actions where dateCompleted is not null
INSERT INTO #userActionData
(
	submissionId,
	actionType,
	userActionId,
	userId,
	establishmentId,
	matEstablishmentId,
	requestedUrl,
	dateCreated
)
SELECT
	sd.submissionId,
	'Completion',
	NEWID(),
	sd.lastResponderId,
	sd.establishmentId,
	sd.updatedMatEstablishmentId,
	'submissionId=' + CAST(sd.submissionId AS NVARCHAR(30)),
	sd.dateCompleted
FROM
	#submissionData sd
WHERE
	sd.dateCompleted IS NOT NULL;

SELECT @completionUserActionCount = COUNT(*) - @creationUserActionCount - @updateUserActionCount FROM #userActionData;

SELECT
	@creationUserActionCount AS creationUserActionCount,
	@updateUserActionCount AS updateUserActionCount,
	@completionUserActionCount AS completionUserActionCount;

--*/

/*******************************************************************************
* BLOCK 2: Write user actions to dbo.userAction and associate with submissions *
*******************************************************************************/

/*
BEGIN TRAN;

-- Insert the user actions into dbo.userAction
INSERT INTO dbo.userAction
(
	id, userId, establishmentId, matEstablishmentId, requestedUrl, dateCreated
)
SELECT
	uad.userActionId,
	uad.userId,
	uad.establishmentId,
	uad.matEstablishmentId,
	uad.requestedUrl,
	uad.dateCreated
FROM
	#userActionData uad
	INNER JOIN dbo.submission s ON s.id = uad.submissionId
WHERE
	(uad.actionType = 'Creation'   AND s.createdUserActionId IS NULL) OR
	(uad.actionType = 'Update'     AND s.lastUpdatedUserActionId IS NULL) OR
	(uad.actionType = 'Completion' AND s.completedUserActionId IS NULL);

-- Show record counts before UPDATEs
SELECT
	SUM(CASE WHEN s.createdUserActionId IS NULL THEN 0 ELSE 1 END) AS createdUserActionCountBefore,
	SUM(CASE WHEN s.lastUpdatedUserActionId IS NULL THEN 0 ELSE 1 END) AS lastUpdatedUserActionCountBefore,
	SUM(CASE WHEN s.completedUserActionId IS NULL THEN 0 ELSE 1 END) AS completedUserActionCountBefore
FROM
	dbo.submission s;

-- Associate dbo.submission.createdUserActionId with the creation actions from #userActionData
UPDATE
	s
SET
	s.createdUserActionId = uad.userActionid
FROM
	dbo.submission s
	INNER JOIN #userActionData uad ON uad.submissionId = s.id
WHERE
	s.createdUserActionId IS NULL AND
	uad.actionType = 'Creation';

-- Associate dbo.submission.lastUpdatedUserActionId with the creation actions from #userActionData
UPDATE
	s
SET
	s.lastUpdatedUserActionId = uad.userActionid
FROM
	dbo.submission s
	INNER JOIN #userActionData uad ON uad.submissionId = s.id
WHERE
	s.lastUpdatedUserActionId IS NULL AND
	uad.actionType = 'Update';

-- Associate dbo.submission.completedUserActionId with the creation actions from #userActionData
UPDATE
	s
SET
	s.completedUserActionId = uad.userActionid
FROM
	dbo.submission s
	INNER JOIN #userActionData uad ON uad.submissionId = s.id
WHERE
	s.completedUserActionId IS NULL AND
	uad.actionType = 'Completion';

-- Show record counts after UPDATEs
SELECT
	SUM(CASE WHEN s.createdUserActionId IS NULL THEN 0 ELSE 1 END) AS createdUserActionCountAfter,
	SUM(CASE WHEN s.lastUpdatedUserActionId IS NULL THEN 0 ELSE 1 END) AS lastUpdatedUserActionCountAfter,
	SUM(CASE WHEN s.completedUserActionId IS NULL THEN 0 ELSE 1 END) AS completedUserActionCountAfter
FROM
	dbo.submission s;

WITH userActionEstablishmentNames AS
(
	SELECT
		ua.id,
		ua.userId,
		schE.orgName AS schoolName,
		matE.orgName AS matName
	FROM
		dbo.userAction ua
		JOIN dbo.establishment schE ON schE.id = ua.establishmentId
		LEFT JOIN dbo.establishment matE ON matE.id = ua.matEstablishmentId
)

SELECT TOP 1000
	s.id AS submissionId,
	s.sectionName,
	uaCreated.userId AS creationUserId,
	uaCreated.schoolName AS creationSchoolName,
	uaCreated.matName AS creationMatName,
	uaUpdated.userId AS updateUserId,
	uaUpdated.schoolName AS updateSchoolName,
	uaUpdated.matName AS updateMatName,
	uaCompleted.userId AS completionUserId,
	uaCompleted.schoolName AS completionSchoolName,
	uaCompleted.matName AS completionMatName
FROM
	dbo.submission s
	JOIN userActionEstablishmentNames uaCreated ON uaCreated.id = s.createdUserActionId
	JOIN userActionEstablishmentNames uaUpdated ON uaUpdated.id = s.lastUpdatedUserActionId
	JOIN userActionEstablishmentNames uaCompleted ON uaCompleted.id = s.completedUserActionId;

ROLLBACK TRAN;
--*/

/*******************************************************************************
* BLOCK 3: Clean up                                                            *
*******************************************************************************/

/*
IF OBJECT_ID(N'tempdb..#submissionData', N'U') IS NOT NULL
  DROP TABLE #submissionData;

IF OBJECT_ID(N'tempdb..#userActionData', N'U') IS NOT NULL
  DROP TABLE #userActionData;
--*/
