BEGIN TRANSACTION;
GO

ALTER TABLE [Contentful].[RecommendationIntros]
    ADD [Slug] nvarchar(max) NOT NULL;

COMMIT;
GO
