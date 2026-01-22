CREATE
OR ALTER VIEW [Contentful].[RichTextContentsBySubtopicRecommendationId] AS (
  SELECT
    [ContentComponentId],
    [RichTextContentsWithSlug].[RichTextId],
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
GO
