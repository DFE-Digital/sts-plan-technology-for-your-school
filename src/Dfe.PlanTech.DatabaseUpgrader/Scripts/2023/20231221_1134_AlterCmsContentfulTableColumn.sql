BEGIN TRAN

    ALTER TABLE [Contentful].[Categories]
    ADD COLUMN [InternalName] nvarchar(max) NOT NULL;

COMMIT TRAN