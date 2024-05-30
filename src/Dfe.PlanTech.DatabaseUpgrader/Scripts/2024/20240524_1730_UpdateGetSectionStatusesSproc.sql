ALTER PROCEDURE GetSectionStatuses
    @splitStringVal NVARCHAR(255),
    @establishmentId INT
AS

SELECT value AS sectionId
INTO #SectionIds
FROM STRING_SPLIT(@splitStringVal, ',')

SELECT
    Sub.sectionId,
    CAST(Sub.completed AS INT) completed,
    Sub.maturity,
    Sub.dateCreated
From #SectionIds Ids
CROSS APPLY (
    SELECT TOP 1 sectionId, completed, maturity, dateCreated
    FROM [dbo].submission S
    WHERE
        Ids.sectionId = S.sectionId
      AND S.establishmentId = @establishmentId
      AND S.deleted = 0
    ORDER BY S.dateCreated DESC
) Sub
