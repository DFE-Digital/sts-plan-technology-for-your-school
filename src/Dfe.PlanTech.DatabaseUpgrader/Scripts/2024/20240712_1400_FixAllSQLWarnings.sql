ALTER VIEW [Contentful].[RichTextContentsBySubtopicRecommendationId] AS
    WITH RecContents AS (
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
    ),
    TextContent AS (
        SELECT
            [RichTextId],
            ISNULL([Warning].[Id], [TB].[Id]) AS ContentId
        FROM
            [Contentful].[TextBodies] AS TB
            LEFT JOIN [Contentful].[Warnings] AS Warning ON [Warning].[TextId] = [TB].[Id]
        WHERE
            [RichTextId] IS NOT NULL
    )
    SELECT
        [ContentComponentId],
        [RichTextContentsWithSlug].[RichTextId] AS Id,
        [SubtopicRecommendationId],
        [Value],
        [NodeType],
        [DataId],
        [ParentId]
    FROM RecContents
    LEFT JOIN TextContent ON RecContents.ContentComponentId = TextContent.ContentId
    OUTER APPLY (
        SELECT
            [Id] AS [RichTextId],
            [Value],
            [NodeType],
            [DataId],
            [ParentId]
        FROM
            [Contentful].[SelectAllRichTextContentForParentId]([TextContent].[RichTextId])
    ) AS RichTextContentsWithSlug

GO

ALTER VIEW [Contentful].[RichTextContentsBySlug] AS
    WITH RichTexts AS (
        --- Get RichTextIds for all text bodies and warnings
        SELECT
            [RichTextId],
            ISNULL([Warning].[Id], [TB].[Id]) AS ContentId
        FROM
            [Contentful].[TextBodies] AS TB
            LEFT JOIN [Contentful].Warnings AS Warning ON [Warning].[TextId] = [TB].[Id]
    ),
    Contents AS (
        SELECT
            MAX([Slug]) AS [Slug],
            [ContentId]
        FROM
            (
                SELECT
                    ISNULL([BeforeContentComponentId], [ContentComponentId]) AS [ContentId],
                    [P].[Slug]
                FROM
                    [Contentful].[PageContents] PC
                    LEFT JOIN [Contentful].[Pages] P ON [PC].[PageId] = [P].[Id]
            ) AS PC
        GROUP BY
            [PC].[ContentId]
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
        FROM
            [Contentful].[SelectAllRichTextContentForParentId]([RichTextContents].[RichTextId])
    ) AS RichTextContentsWithSlug

GO
