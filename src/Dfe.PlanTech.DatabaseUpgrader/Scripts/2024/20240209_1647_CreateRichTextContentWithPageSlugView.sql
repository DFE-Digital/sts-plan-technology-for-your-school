ALTER VIEW Contentful.[RichTextContentsBySlug] AS
(
    SELECT [Slug], [Id], [Value], [NodeType], [DataId], [ParentId], [Published], [Archived], [Deleted]
    FROM (     
        SELECT * 
        FROM (
                SELECT RichTextId, Slug, Archived, Deleted, Published
                FROM 
                (
                    --- Get RichTextIds for all text bodies and warnings
                    SELECT * 
                    FROM 
                    (
                        SELECT RichTextId, Id AS ContentId 
                        FROM [Contentful].[TextBodies] AS TB
                        UNION
                        SELECT RichTextId, TB.Id 
                        FROM [Contentful].[Warnings] AS W
                        LEFT JOIN [Contentful].[TextBodies] AS TB ON W.TextId = TB.Id
                    ) AS RT
                ) AS RichTexts

                LEFT JOIN 
                
                --Get all contents for a page
                (
                    SELECT MAX(Slug) AS Slug, ContentId
                    FROM 
                    (
                        SELECT ISNULL([BeforeContentComponentId], [ContentComponentId]) AS ContentId, P.Slug 
                        FROM [Contentful].[PageContents] PC
                        LEFT JOIN [Contentful].[Pages] P ON PC.PageId = P.Id
                    ) AS PC
                    GROUP BY PC.ContentId
                ) AS Contents ON Contents.ContentId = RichTexts.ContentId

                LEFT JOIN(
                    SELECT Id, Archived, Published, DELETED
                    FROM Contentful.ContentComponents
                ) AS CC 
                ON CC.Id = RichTexts.ContentId
            ) AS RichTextContents
        ) AS RichTextContentParents
    OUTER APPLY (
        SELECT * FROM [Contentful].[SelectAllRichTextContentForParentId](RichTextContentParents.RichTextId) 
    ) AS RichTextContentsWithSlug
)
