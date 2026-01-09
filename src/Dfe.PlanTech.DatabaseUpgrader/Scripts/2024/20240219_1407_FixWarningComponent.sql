CREATE
OR ALTER VIEW [Contentful].[RichTextContentsBySlug] AS (
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
      FROM
        (
          --- Get RichTextIds for all text bodies and warnings
          SELECT
            [RichTextId],
            ISNULL([Warning].[Id], [TB].[Id]) AS ContentId
          FROM
            [Contentful].[TextBodies] AS TB
            LEFT JOIN [Contentful].Warnings AS Warning ON [Warning].[TextId] = [TB].[Id]
        ) AS RichTexts
        LEFT JOIN --Get all contents for a page
        (
          SELECT
            MAX([Slug]) AS [Slug],
            [ContentId]
          FROM
            (
              SELECT
                ISNULL(
                  [BeforeContentComponentId],
                  [ContentComponentId]
                ) AS [ContentId],
                [P].[Slug]
              FROM
                [Contentful].[PageContents] PC
                LEFT JOIN [Contentful].[Pages] P ON [PC].[PageId] = [P].[Id]
            ) AS PC
          GROUP BY
            [PC].[ContentId]
        ) AS Contents ON [Contents].[ContentId] = [RichTexts].[ContentId]
        LEFT JOIN(
          SELECT
            [Id],
            [Archived],
            [Published],
            [DELETED]
          FROM
            [Contentful].[ContentComponents]
        ) AS CC ON [CC].[Id] = [RichTexts].[ContentId]
    ) AS RichTextContents
    OUTER APPLY (
      SELECT
        [Id],
        [Value],
        [NodeType],
        [DataId],
        [ParentId]
      FROM
        [Contentful].[SelectAllRichTextContentForParentId]([RichTextContents].[RichTextId])
    ) AS RichTextContentsWithSlug
)
