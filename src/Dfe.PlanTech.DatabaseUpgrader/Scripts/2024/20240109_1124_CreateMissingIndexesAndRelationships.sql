BEGIN TRANSACTION;
GO

IF SCHEMA_ID(N'Contentful') IS NULL EXEC(N'CREATE SCHEMA [Contentful];');
GO

-- Your existing table creation statements go here...

-- Add foreign key constraints for tables with "Id varchar(30)" column
-- The key name should be "FK_TABLENAME_ContentComponents_Id"

-- Buttons
ALTER TABLE [Contentful].[Buttons]
ADD CONSTRAINT [FK_Buttons_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

CREATE INDEX [IX_Buttons_Id] ON [Contentful].[Buttons] ([Id]);
GO

-- Headers
ALTER TABLE [Contentful].[Headers]
ADD CONSTRAINT [FK_Headers_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

CREATE INDEX [IX_Headers_Id] ON [Contentful].[Headers] ([Id]);
GO

-- InsetTexts
ALTER TABLE [Contentful].[InsetTexts]
ADD CONSTRAINT [FK_InsetTexts_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO 

CREATE INDEX [IX_InsetTexts_Id] ON [Contentful].[InsetTexts] ([Id]);
GO

-- NavigationLink
ALTER TABLE [Contentful].[NavigationLink]
ADD CONSTRAINT [FK_NavigationLink_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

CREATE INDEX [IX_NavigationLink_Id] ON [Contentful].[NavigationLink] ([Id]);
GO

-- Titles
ALTER TABLE [Contentful].[Titles]
ADD CONSTRAINT [FK_Titles_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

CREATE INDEX [IX_Titles_Id] ON [Contentful].[Titles] ([Id]);
GO

-- RichTextContents
ALTER TABLE [Contentful].[RichTextContents]
ADD CONSTRAINT [FK_RichTextContents_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

CREATE INDEX [IX_RichTextContents_Id] ON [Contentful].[RichTextContents] ([Id]);
GO

-- TextBodies
ALTER TABLE [Contentful].[TextBodies]
ADD CONSTRAINT [FK_TextBodies_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

CREATE INDEX [IX_TextBodies_Id] ON [Contentful].[TextBodies] ([Id]);
GO

-- Warnings
ALTER TABLE [Contentful].[Warnings]
ADD CONSTRAINT [FK_Warnings_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

CREATE INDEX [IX_Warnings_Id] ON [Contentful].[Warnings] ([Id]);
GO

COMMIT;
GO
