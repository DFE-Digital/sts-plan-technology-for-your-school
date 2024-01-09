BEGIN TRANSACTION;
GO

-- Buttons
ALTER TABLE [Contentful].[Buttons]
ADD CONSTRAINT [FK_Buttons_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

-- ComponentDropdowns
ALTER TABLE [Contentful].[ComponentDropdowns]
ADD CONSTRAINT [FK_ComponentDropdowns_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;

-- Headers
ALTER TABLE [Contentful].[Headers]
ADD CONSTRAINT [FK_Headers_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

-- InsetTexts
ALTER TABLE [Contentful].[InsetTexts]
ADD CONSTRAINT [FK_InsetTexts_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO 

-- NavigationLink
ALTER TABLE [Contentful].[NavigationLink]
ADD CONSTRAINT [FK_NavigationLink_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

-- Warnings
ALTER TABLE [Contentful].[Warnings]
ADD CONSTRAINT [FK_Warnings_ContentComponents_Id]
FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE;
GO

COMMIT;
GO
