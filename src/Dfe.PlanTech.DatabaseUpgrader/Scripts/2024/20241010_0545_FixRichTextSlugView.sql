ALTER VIEW [Contentful].[RichTextContentsBySlug] AS
    WITH RichTexts AS (
        SELECT
            [RichTextId],
            ISNULL([Warning].[Id], [TB].[Id]) AS ContentId
        FROM
            [Contentful].[TextBodies] AS TB
            LEFT JOIN [Contentful].Warnings AS Warning ON [Warning].[TextId] = [TB].[Id]
    ),
    Contents AS (
        SELECT
            ISNULL([BeforeContentComponentId], [ContentComponentId]) AS [ContentId],
            [P].[Slug]
        FROM
            [Contentful].[PageContents] PC
            LEFT JOIN [Contentful].[Pages] P ON [PC].[PageId] = [P].[Id]
    )
SELECT
    [Slug],
    [Id],
    [Value],
    [NodeType],
    [DataId],
    [ParentId],
    [Published],
    [Archived],
    [Deleted]
FROM
(
    SELECT
        [RichTextId],
        [Slug],
        [Archived],
        [Deleted],
        [Published]
    FROM RichTexts
    LEFT JOIN Contents ON [Contents].[ContentId] = [RichTexts].[ContentId]
    LEFT JOIN [Contentful].[ContentComponents] CC ON [CC].[Id] = [RichTexts].[ContentId]
) AS RichTextContents
OUTER APPLY (
    SELECT
        [Id],
        [Value],
        [NodeType],
        [DataId],
        [ParentId]
    FROM [Contentful].[SelectAllRichTextContentForParentId]([RichTextContents].[RichTextId])
) AS RichTextContentsWithSlug
GO
