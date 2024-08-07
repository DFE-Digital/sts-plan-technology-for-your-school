ALTER TABLE Contentful.[Sections] DROP CONSTRAINT FK_Sections_Pages_InterstitialPageId;
ALTER TABLE Contentful.[Warnings] DROP CONSTRAINT FK_Warnings_TextBodies_TextId;
ALTER TABLE Contentful.[PageContents] DROP CONSTRAINT FK_PageContents_Pages_PageId;
ALTER TABLE Contentful.[ComponentDropDowns] DROP CONSTRAINT FK_ComponentDropDowns_RichTextContents_RichTextContentId;
ALTER TABLE Contentful.[TextBodies] DROP CONSTRAINT FK_TextBodies_RichTextContents_RichTextId;
GO

DROP INDEX IX_Sections_InterstitialPageId ON Contentful.[Sections]
DROP INDEX IX_Warnings_TextId ON Contentful.[Warnings]
DROP INDEX IX_PageContents_BeforeContentComponentId_PageId ON Contentful.[PageContents]
DROP INDEX IX_ComponentDropDowns_RichTextContentId ON Contentful.[ComponentDropDowns]
DROP INDEX IX_TextBodies_RichTextId ON Contentful.[TextBodies]
GO

ALTER TABLE Contentful.[Sections] ALTER COLUMN [InterstitialPageId] NVARCHAR(30) NULL;
ALTER TABLE Contentful.[Warnings] ALTER COLUMN [TextId] NVARCHAR(30) NULL;
ALTER TABLE Contentful.[Questions] ALTER COLUMN [Text] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[Questions] ALTER COLUMN [Slug] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[Answers] ALTER COLUMN [Text] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[Answers] ALTER COLUMN [Maturity] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[RecommendationIntros] ALTER COLUMN [Maturity] NVARCHAR(40) NULL;
ALTER TABLE Contentful.[RecommendationIntros] ALTER COLUMN [Slug] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[PageContents] ALTER COLUMN [PageId] NVARCHAR(30) NULL;
ALTER TABLE Contentful.[Buttons] ALTER COLUMN [Value] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[Buttons] ALTER COLUMN [IsStartButton] BIT NULL;
ALTER TABLE Contentful.[Headers] ALTER COLUMN [Text] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[Headers] ALTER COLUMN [Tag] INT NULL;
ALTER TABLE Contentful.[Headers] ALTER COLUMN [Size] INT NULL;
ALTER TABLE Contentful.[InsetTexts] ALTER COLUMN [Text] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[NavigationLink] ALTER COLUMN [DisplayText] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[NavigationLink] ALTER COLUMN [Href] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[NavigationLink] ALTER COLUMN [OpenInNewTab] BIT NULL;
ALTER TABLE Contentful.[ButtonWithLinks] ALTER COLUMN [Href] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[Titles] ALTER COLUMN [Text] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[Categories] ALTER COLUMN [InternalName] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[RichTextContents] ALTER COLUMN [Value] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[RichTextContents] ALTER COLUMN [NodeType] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[Pages] ALTER COLUMN [InternalName] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[Pages] ALTER COLUMN [Slug] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[Pages] ALTER COLUMN [DisplayBackButton] BIT NULL;
ALTER TABLE Contentful.[Pages] ALTER COLUMN [DisplayHomeButton] BIT NULL;
ALTER TABLE Contentful.[Pages] ALTER COLUMN [DisplayTopicTitle] BIT NULL;
ALTER TABLE Contentful.[Pages] ALTER COLUMN [DisplayOrganisationName] BIT NULL;
ALTER TABLE Contentful.[Pages] ALTER COLUMN [RequiresAuthorisation] BIT NULL;
ALTER TABLE Contentful.[CSLinks] ALTER COLUMN [LinkText] NVARCHAR(256) NULL;
ALTER TABLE Contentful.[CSLinks] ALTER COLUMN [Url] NVARCHAR(256) NULL;
ALTER TABLE Contentful.[ComponentDropDowns] ALTER COLUMN [Title] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[ComponentDropDowns] ALTER COLUMN [RichTextContentId] BIGINT NULL;
ALTER TABLE Contentful.[RichTextMarkDbEntity] ALTER COLUMN [Type] NVARCHAR(MAX) NULL;
ALTER TABLE Contentful.[TextBodies] ALTER COLUMN [RichTextId] BIGINT NULL;
GO

CREATE UNIQUE INDEX IX_Sections_InterstitialPageId On Contentful.[Sections](InterstitialPageId) WHERE [InterstitialPageId] IS NOT NULL;
CREATE INDEX IX_Warnings_TextId On Contentful.[Warnings](TextId);
CREATE INDEX IX_PageContents_BeforeContentComponentId_PageId On Contentful.[PageContents] (BeforeContentComponentId) include (PageId)
CREATE UNIQUE INDEX IX_ComponentDropDowns_RichTextContentId On Contentful.[ComponentDropDowns](RichTextContentId) WHERE [RichTextContentId] IS NOT NULL;
CREATE INDEX IX_TextBodies_RichTextId On Contentful.[TextBodies](RichTextId);
GO

ALTER TABLE Contentful.[Sections] ADD CONSTRAINT FK_Sections_Pages_InterstitialPageId FOREIGN KEY (InterstitialPageId) REFERENCES Contentful.Pages(Id);
ALTER TABLE Contentful.[Warnings] ADD CONSTRAINT FK_Warnings_TextBodies_TextId FOREIGN KEY (TextId) REFERENCES Contentful.TextBodies(Id);
ALTER TABLE Contentful.[PageContents] ADD CONSTRAINT FK_PageContents_Pages_PageId FOREIGN KEY (PageId) REFERENCES Contentful.Pages(Id);
ALTER TABLE Contentful.[ComponentDropDowns] ADD CONSTRAINT FK_ComponentDropDowns_RichTextContents_RichTextContentId FOREIGN KEY (RichTextContentId) REFERENCES Contentful.RichTextContents(Id) ON DELETE CASCADE;
ALTER TABLE Contentful.[TextBodies] ADD CONSTRAINT FK_TextBodies_RichTextContents_RichTextId FOREIGN KEY (RichTextId) REFERENCES Contentful.RichTextContents(Id) ON DELETE CASCADE;
GO
