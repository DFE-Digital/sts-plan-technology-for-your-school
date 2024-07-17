IF EXISTS(
    SELECT 1 FROM sys.columns SC
             JOIN sys.tables ST on SC.object_id = ST.object_id
             JOIN sys.schemas SS on SS.schema_id = ST.schema_id
    WHERE
        SS.name = 'Contentful'
    AND ST.name = 'RecommendationChunks'
    AND SC.name = 'Title'
)
BEGIN
    ALTER TABLE [Contentful].[RecommendationChunks] DROP COLUMN [Title]
END
