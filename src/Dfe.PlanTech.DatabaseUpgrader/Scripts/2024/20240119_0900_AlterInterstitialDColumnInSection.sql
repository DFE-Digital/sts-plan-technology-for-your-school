BEGIN TRANSACTION;
GO

DROP INDEX IF EXISTS [IX_Sections_InterstitialPageId] ON [Contentful].[Sections];

ALTER TABLE [Contentful].[Sections]
ALTER COLUMN [InterstitialPageId] nvarchar(30) NOT NULL;

CREATE UNIQUE INDEX [IX_Sections_InterstitialPageId] ON [Contentful].[Sections] ([InterstitialPageId]) WHERE [InterstitialPageId] IS NOT NULL;

COMMIT;
GO
