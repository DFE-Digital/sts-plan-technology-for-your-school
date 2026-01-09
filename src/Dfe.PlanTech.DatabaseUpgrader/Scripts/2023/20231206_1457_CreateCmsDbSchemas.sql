BEGIN TRANSACTION;
GO

IF SCHEMA_ID(N'Contentful') IS NULL EXEC(N'CREATE SCHEMA [Contentful];');
GO

CREATE TABLE [Contentful].[Buttons] (
    [Id] nvarchar(30) NOT NULL,
    [Value] nvarchar(max) NOT NULL,
    [IsStartButton] bit NOT NULL,
    CONSTRAINT [PK_Buttons] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Contentful].[ContentComponents] (
    [Id] nvarchar(30) NOT NULL,
    [Published] bit NOT NULL,
    [Archived] bit NOT NULL,
    [Deleted] bit NOT NULL,
    CONSTRAINT [PK_ContentComponents] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Contentful].[Headers] (
    [Id] nvarchar(30) NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [Tag] int NOT NULL,
    [Size] int NOT NULL,
    CONSTRAINT [PK_Headers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Contentful].[InsetTexts] (
    [Id] nvarchar(30) NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_InsetTexts] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Contentful].[NavigationLink] (
    [Id] nvarchar(30) NOT NULL,
    [DisplayText] nvarchar(max) NOT NULL,
    [Href] nvarchar(max) NOT NULL,
    [OpenInNewTab] bit NOT NULL,
    CONSTRAINT [PK_NavigationLink] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Contentful].[RichTextDataDbEntity] (
    [Id] bigint NOT NULL IDENTITY,
    [Uri] nvarchar(max) NULL,
    CONSTRAINT [PK_RichTextDataDbEntity] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Contentful].[ButtonWithLinks] (
    [Id] nvarchar(30) NOT NULL,
    [ButtonId] nvarchar(30) NULL,
    [Href] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_ButtonWithLinks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ButtonWithLinks_Buttons_ButtonId] FOREIGN KEY ([ButtonId]) REFERENCES [Contentful].[Buttons] ([Id])
);
GO

CREATE TABLE [Contentful].[ButtonWithEntryReferences] (
    [Id] nvarchar(30) NOT NULL,
    [ButtonId] nvarchar(30) NULL,
    [LinkToEntryId] nvarchar(30) NULL,
    CONSTRAINT [PK_ButtonWithEntryReferences] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ButtonWithEntryReferences_Buttons_ButtonId] FOREIGN KEY ([ButtonId]) REFERENCES [Contentful].[Buttons] ([Id]),
    CONSTRAINT [FK_ButtonWithEntryReferences_ContentComponents_LinkToEntryId] FOREIGN KEY ([LinkToEntryId]) REFERENCES [Contentful].[ContentComponents] ([Id])
);
GO

CREATE TABLE [Contentful].[Titles] (
    [Id] nvarchar(30) NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Titles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Titles_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Contentful].[Categories] (
    [Id] nvarchar(30) NOT NULL,
    [HeaderId] nvarchar(30) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Categories_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Categories_Headers_HeaderId] FOREIGN KEY ([HeaderId]) REFERENCES [Contentful].[Headers] ([Id])
);
GO

CREATE TABLE [Contentful].[RichTextContents] (
    [Id] bigint NOT NULL IDENTITY,
    [Value] nvarchar(max) NOT NULL,
    [NodeType] nvarchar(max) NOT NULL,
    [DataId] bigint NULL,
    [ParentId] bigint NULL,
    CONSTRAINT [PK_RichTextContents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RichTextContents_RichTextContents_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Contentful].[RichTextContents] ([Id]),
    CONSTRAINT [FK_RichTextContents_RichTextDataDbEntity_DataId] FOREIGN KEY ([DataId]) REFERENCES [Contentful].[RichTextDataDbEntity] ([Id])
);
GO

CREATE TABLE [Contentful].[Pages] (
    [Id] nvarchar(30) NOT NULL,
    [InternalName] nvarchar(max) NOT NULL,
    [Slug] nvarchar(max) NOT NULL,
    [DisplayBackButton] bit NOT NULL,
    [DisplayHomeButton] bit NOT NULL,
    [DisplayTopicTitle] bit NOT NULL,
    [DisplayOrganisationName] bit NOT NULL,
    [RequiresAuthorisation] bit NOT NULL,
    [TitleId] nvarchar(30) NULL,
    [SectionId] nvarchar(max) NULL,
    CONSTRAINT [PK_Pages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Pages_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Pages_Titles_TitleId] FOREIGN KEY ([TitleId]) REFERENCES [Contentful].[Titles] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Contentful].[ComponentDropDowns] (
    [Id] nvarchar(30) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [RichTextContentId] bigint NOT NULL,
    CONSTRAINT [PK_ComponentDropDowns] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ComponentDropDowns_RichTextContents_RichTextContentId] FOREIGN KEY ([RichTextContentId]) REFERENCES [Contentful].[RichTextContents] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Contentful].[RichTextMarkDbEntity] (
    [Id] bigint NOT NULL IDENTITY,
    [Type] nvarchar(max) NOT NULL,
    [RichTextContentDbEntityId] bigint NULL,
    CONSTRAINT [PK_RichTextMarkDbEntity] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RichTextMarkDbEntity_RichTextContents_RichTextContentDbEntityId] FOREIGN KEY ([RichTextContentDbEntityId]) REFERENCES [Contentful].[RichTextContents] ([Id])
);
GO

CREATE TABLE [Contentful].[TextBodies] (
    [Id] nvarchar(30) NOT NULL,
    [RichTextId] bigint NOT NULL,
    CONSTRAINT [PK_TextBodies] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TextBodies_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TextBodies_RichTextContents_RichTextId] FOREIGN KEY ([RichTextId]) REFERENCES [Contentful].[RichTextContents] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Contentful].[PageContents] (
    [Id] bigint NOT NULL PRIMARY KEY IDENTITY,
    [BeforeContentComponentId] nvarchar(30) NULL,
    [ContentComponentId] nvarchar(30) NULL,
    [PageId] nvarchar(30) NOT NULL,
    CONSTRAINT [FK_PageContents_ContentComponents_BeforeContentComponentId] FOREIGN KEY ([BeforeContentComponentId]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PageContents_ContentComponents_ContentComponentId] FOREIGN KEY ([ContentComponentId]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PageContents_Pages_PageId] FOREIGN KEY ([PageId]) REFERENCES [Contentful].[Pages] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Contentful].[Sections] (
    [Id] nvarchar(30) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [InterstitialPageId] nvarchar(30) NULL,
    [CategoryId] nvarchar(30) NULL,
    CONSTRAINT [PK_Sections] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Sections_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Contentful].[Categories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Sections_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Sections_Pages_InterstitialPageId] FOREIGN KEY ([InterstitialPageId]) REFERENCES [Contentful].[Pages] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Contentful].[Warnings] (
    [Id] nvarchar(30) NOT NULL,
    [TextId] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_Warnings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Warnings_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Warnings_TextBodies_TextId] FOREIGN KEY ([TextId]) REFERENCES [Contentful].[TextBodies] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Contentful].[Questions] (
    [Id] nvarchar(30) NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [HelpText] nvarchar(max) NULL,
    [Slug] nvarchar(max) NOT NULL,
    [SectionId] nvarchar(30) NULL,
    CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Questions_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Questions_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Contentful].[Sections] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Contentful].[RecommendationPages] (
    [Id] nvarchar(30) NOT NULL,
    [InternalName] nvarchar(max) NOT NULL,
    [DisplayName] nvarchar(max) NOT NULL,
    [Maturity] int NOT NULL,
    [PageId] nvarchar(30) NOT NULL,
    [SectionId] nvarchar(30) NULL,
    CONSTRAINT [PK_RecommendationPages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecommendationPages_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RecommendationPages_Pages_PageId] FOREIGN KEY ([PageId]) REFERENCES [Contentful].[Pages] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RecommendationPages_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Contentful].[Sections] ([Id])
);
GO

CREATE TABLE [Contentful].[Answers] (
    [Id] nvarchar(30) NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [NextQuestionId] nvarchar(30) NULL,
    [Maturity] nvarchar(max) NOT NULL,
    [ParentQuestionId] nvarchar(30) NULL,
    CONSTRAINT [PK_Answers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Answers_ContentComponents_Id] FOREIGN KEY ([Id]) REFERENCES [Contentful].[ContentComponents] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Answers_Questions_NextQuestionId] FOREIGN KEY ([NextQuestionId]) REFERENCES [Contentful].[Questions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Answers_Questions_ParentQuestionId] FOREIGN KEY ([ParentQuestionId]) REFERENCES [Contentful].[Questions] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Answers_NextQuestionId] ON [Contentful].[Answers] ([NextQuestionId]);
GO

CREATE INDEX [IX_Answers_ParentQuestionId] ON [Contentful].[Answers] ([ParentQuestionId]);
GO

CREATE INDEX [IX_ButtonWithEntryReferences_ButtonId] ON [Contentful].[ButtonWithEntryReferences] ([ButtonId]);
GO

CREATE INDEX [IX_ButtonWithEntryReferences_LinkToEntryId] ON [Contentful].[ButtonWithEntryReferences] ([LinkToEntryId]);
GO

CREATE INDEX [IX_ButtonWithLinks_ButtonId] ON [Contentful].[ButtonWithLinks] ([ButtonId]);
GO

CREATE INDEX [IX_Categories_HeaderId] ON [Contentful].[Categories] ([HeaderId]);
GO

CREATE UNIQUE INDEX [IX_ComponentDropDowns_RichTextContentId] ON [Contentful].[ComponentDropDowns] ([RichTextContentId]) WHERE [RichTextContentId] IS NOT NULL;
GO

CREATE INDEX [IX_PageContents_BeforeContentComponentId] ON [Contentful].[PageContents] ([BeforeContentComponentId]);
GO

CREATE INDEX [IX_PageContents_ContentComponentId] ON [Contentful].[PageContents] ([ContentComponentId]);
GO

CREATE INDEX [IX_PageContents_PageId] ON [Contentful].[PageContents] ([PageId]);
GO

CREATE INDEX [IX_Pages_TitleId] ON [Contentful].[Pages] ([TitleId]);
GO

CREATE INDEX [IX_Questions_SectionId] ON [Contentful].[Questions] ([SectionId]);
GO

CREATE UNIQUE INDEX [IX_RecommendationPages_PageId] ON [Contentful].[RecommendationPages] ([PageId]) WHERE [PageId] IS NOT NULL;
GO

CREATE INDEX [IX_RecommendationPages_SectionId] ON [Contentful].[RecommendationPages] ([SectionId]);
GO

CREATE INDEX [IX_RichTextContents_DataId] ON [Contentful].[RichTextContents] ([DataId]);
GO

CREATE INDEX [IX_RichTextContents_ParentId] ON [Contentful].[RichTextContents] ([ParentId]);
GO

CREATE INDEX [IX_RichTextMarkDbEntity_RichTextContentDbEntityId] ON [Contentful].[RichTextMarkDbEntity] ([RichTextContentDbEntityId]);
GO

CREATE INDEX [IX_Sections_CategoryId] ON [Contentful].[Sections] ([CategoryId]);
GO

CREATE UNIQUE INDEX [IX_Sections_InterstitialPageId] ON [Contentful].[Sections] ([InterstitialPageId]) WHERE [InterstitialPageId] IS NOT NULL;
GO

CREATE INDEX [IX_TextBodies_RichTextId] ON [Contentful].[TextBodies] ([RichTextId]);
GO

CREATE INDEX [IX_Warnings_TextId] ON [Contentful].[Warnings] ([TextId]);
GO

COMMIT;
GO
