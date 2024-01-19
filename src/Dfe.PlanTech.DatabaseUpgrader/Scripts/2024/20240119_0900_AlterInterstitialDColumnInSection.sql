BEGIN TRANSACTION;
GO

ALTER TABLE [Contentful].[Sections]
ALTER COLUMN [InterstitialPageId] nvarchar(30) NOT NULL;

COMMIT;
GO