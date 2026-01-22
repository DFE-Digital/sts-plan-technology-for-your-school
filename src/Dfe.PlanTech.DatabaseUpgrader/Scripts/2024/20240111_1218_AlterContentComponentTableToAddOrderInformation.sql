BEGIN TRANSACTION;
GO

-- Track the order that a piece of content should appear when being consumed.
ALTER TABLE [Contentful].[ContentComponents]
ADD [Order] bigint NULL;

COMMIT;
GO
