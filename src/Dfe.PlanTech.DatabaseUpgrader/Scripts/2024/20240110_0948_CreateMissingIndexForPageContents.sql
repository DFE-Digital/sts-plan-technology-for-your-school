BEGIN TRANSACTION;
GO

  CREATE NONCLUSTERED INDEX [IX_PageContents_BeforeContentComponentId_PageId] ON [Contentful].[PageContents] ([BeforeContentComponentId]) INCLUDE ([PageId]);
  GO

COMMIT;
GO
