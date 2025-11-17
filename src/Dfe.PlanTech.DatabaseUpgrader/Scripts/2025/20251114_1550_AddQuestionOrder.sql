ALTER TABLE dbo.[question]
    ADD [order] INT;

GO

;WITH orderedQuestions AS
          (
              SELECT DISTINCT
                  q.contentfulRef,
                  mq.internalName
              FROM
                  dbo.question q
                      LEFT JOIN migration.questions mq ON q.contentfulRef = mq.contentfulRef
          ),
      referenceOrderMap AS
          (
              SELECT
                  oq.*,
                  TRY_CAST(SUBSTRING(oq.internalName, v.Pos, v.Len) AS INT) AS internalNumber
              FROM orderedQuestions oq
     CROSS APPLY (
     SELECT
     PATINDEX('%[0-9]%', oq.internalName) AS Pos,
     PATINDEX('%[^0-9]%', SUBSTRING(oq.internalName, PATINDEX('%[0-9]%', oq.internalName), LEN(oq.internalName))) AS Len
     ) v
 WHERE
     oq.internalName LIKE 'CORE%'
    OR oq.internalName LIKE 'RETIRED%'
     )

UPDATE
    q
SET
    q.[order] = rom.internalNumber
FROM
    dbo.question q
    JOIN referenceOrderMap rom ON rom.contentfulRef = q.contentfulRef
