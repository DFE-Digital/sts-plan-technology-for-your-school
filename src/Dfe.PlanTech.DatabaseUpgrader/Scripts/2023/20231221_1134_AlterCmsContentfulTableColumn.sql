BEGIN TRAN

    ALTER TABLE [Contentful].[Categories]
    ADD [InternalName] nvarchar(max) NOT NULL;

COMMIT TRAN
