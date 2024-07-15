CREATE
  VIEW [Contentful].[RichTextContentsBySubtopicRecommendationId] AS (
  SELECT
    [ContentComponentId],
    [RichTextContentsWithSlug].[RichTextId] AS Id,
    [SubtopicRecommendationId],
    [Value],
    [NodeType],
    [DataId],
    [ParentId]
  FROM
    (
      (
        SELECT
          SRC.[Id] AS SubtopicRecommendationId,
          RCC.[ContentComponentId]
        FROM
          [Contentful].[SubtopicRecommendations] AS SRC
          JOIN [Contentful].[RecommendationSectionChunks] AS RSC ON SRC.[SectionId] = RSC.[RecommendationSectionId]
          JOIN [Contentful].[RecommendationChunkContents] AS RCC ON RSC.[RecommendationChunkId] = RCC.[RecommendationChunkId]
        UNION
        SELECT
          SRI.[Id] AS SubtopicRecommendationId,
          RIC.[ContentComponentId]
        FROM
          [Contentful].[SubtopicRecommendations] AS SRI
          JOIN [Contentful].[SubtopicRecommendationIntros] AS SRII ON SRI.[Id] = SRII.[SubtopicRecommendationId]
          JOIN [Contentful].[RecommendationIntros] AS RI ON SRII.[RecommendationIntroId] = RI.[Id]
          JOIN [Contentful].[RecommendationIntroContents] AS RIC ON RI.[Id] = RIC.[RecommendationIntroId]
      ) AS RecContents
      LEFT JOIN(
        SELECT
          [RichTextId],
          ISNULL([Warning].[Id], [TB].[Id]) AS ContentId
        FROM
          [Contentful].[TextBodies] AS TB
          LEFT JOIN [Contentful].[Warnings] AS Warning ON [Warning].[TextId] = [TB].[Id]
        WHERE
          [RichTextId] IS NOT NULL
      ) AS TextContent ON RecContents.ContentComponentId = TextContent.ContentId OUTER APPLY (
        SELECT
          [Id] AS [RichTextId],
          [Value],
          [NodeType],
          [DataId],
          [ParentId]
        FROM
          [Contentful].[SelectAllRichTextContentForParentId]([TextContent].[RichTextId])
      ) AS RichTextContentsWithSlug
    )
)
go

CREATE
  VIEW [Contentful].[RichTextContentsBySlug] AS (
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
go

