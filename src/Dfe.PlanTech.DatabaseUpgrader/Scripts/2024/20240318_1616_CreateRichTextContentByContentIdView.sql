
CREATE
  VIEW [Contentful].[RichTextContentIdsWithContentComponentId] AS (
  SELECT
    [Id],
    [ContentId],
    [DataId],
    [ParentId]
      FROM
    (
          SELECT
            [RichTextId],
            [Id] AS [ContentId]
          FROM
            [Contentful].[TextBodies] AS TB
          UNION
          SELECT
            [RichTextId],
            [TB].[Id] as [ContentId]
          FROM
            [Contentful].[Warnings] AS W
            LEFT JOIN [Contentful].[TextBodies] AS TB ON [W].[TextId] = [TB].[Id]
        ) AS RichTexts
    OUTER APPLY (
      SELECT
        [Id],
        [Value],
        [NodeType],
        [DataId],
        [ParentId]
      FROM
        [Contentful].[SelectAllRichTextContentForParentId]([RichTexts].[RichTextId])
    ) AS RichTextContentsWithSlug
)
GO
