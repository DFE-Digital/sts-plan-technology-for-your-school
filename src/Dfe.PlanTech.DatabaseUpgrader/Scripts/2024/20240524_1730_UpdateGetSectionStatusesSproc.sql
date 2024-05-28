ALTER PROCEDURE GetSectionStatuses
    @splitStringVal nvarchar(255),
    @establishmentId int
AS

SELECT value as sectionId
INTO #SectionIds
FROM STRING_SPLIT(@splitStringVal, ',')

Select
    Sub.sectionId,
    CAST(Sub.completed as int) completed,
    Sub.maturity,
    Sub.dateCreated
From #SectionIds Ids
CROSS APPLY (
    Select top 1 sectionId, completed, maturity, dateCreated
    From [dbo].submission S
    Where
        Ids.sectionId = S.sectionId
      AND S.establishmentId = @establishmentId
      AND S.deleted = 0
    ORDER BY S.dateCreated Desc
) Sub
