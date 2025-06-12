WITH
mostRecentCompleted AS (
	SELECT
        s.establishmentId,
        s.sectionId,
        MAX(s.dateCompleted) as dateCompleted
	FROM dbo.submission s
	WHERE s.dateCompleted IS NOT NULL
	GROUP BY s.establishmentId, s.sectionId
),

activeSubmissions AS (
	SELECT
        s.id,
        s.establishmentId,
        s.sectionName,
        s.dateCreated,
        s.dateCompleted
	FROM mostRecentCompleted mrc 
	INNER JOIN dbo.submission s
		ON s.establishmentId = mrc.establishmentId
		AND s.sectionId = mrc.sectionId
		AND s.dateCompleted = mrc.dateCompleted
),

newData AS (
	SELECT
		s.id,
		CASE
            WHEN a.id IS NOT NULL THEN 'CompletedAndReviewed'
            ELSE NULL
        END as status
	FROM dbo.submission s
	LEFT JOIN activeSubmissions a ON a.id = s.id
)

UPDATE s
SET s.status = nd.status
FROM dbo.submission s
INNER JOIN newData nd ON s.id = nd.id

