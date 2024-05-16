-- TODO: Drop this on Dev after testing
-- TODO: Change this to CREATE OR ALTER PROCEDURE dbo.GetSectionStatuses and rename file

CREATE OR ALTER PROCEDURE dbo.GetSectionStatusesForCategory @categoryId nvarchar(30), @establishmentId int
AS
SELECT
    Sub.sectionId,
    Sub.completed,
    Sub.maturity,
    Sub.dateCreated
FROM Contentful.Sections CS
         JOIN Contentful.Categories CC on CC.Id = CS.CategoryId
    CROSS APPLY (
        Select top 1 S.sectionId, S.completed, S.maturity, S.dateCreated
        FROM dbo.submission S
        WHERE 
            S.sectionId = CS.Id
            AND S.establishmentId = @establishmentId
        ORDER BY S.dateCreated Desc
    ) Sub
WHERE
    CC.Id = @categoryId

GO
