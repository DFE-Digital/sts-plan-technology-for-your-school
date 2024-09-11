ALTER TABLE Contentful.[Sections] DROP CONSTRAINT FK_Sections_Pages_InterstitialPageId;
ALTER TABLE Contentful.[Warnings] DROP CONSTRAINT FK_Warnings_TextBodies_TextId;
ALTER TABLE Contentful.[ComponentDropDowns] DROP CONSTRAINT FK_ComponentDropDowns_RichTextContents_RichTextContentId;
ALTER TABLE Contentful.[TextBodies] DROP CONSTRAINT FK_TextBodies_RichTextContents_RichTextId;
GO

DROP INDEX IX_Sections_InterstitialPageId ON Contentful.[Sections]
DROP INDEX IX_Warnings_TextId ON Contentful.[Warnings]
DROP INDEX IX_ComponentDropDowns_RichTextContentId ON Contentful.[ComponentDropDowns]
DROP INDEX IX_TextBodies_RichTextId ON Contentful.[TextBodies]
GO

ALTER TABLE Contentful.[Sections] ALTER COLUMN [InterstitialPageId] NVARCHAR(30) NULL;
ALTER TABLE Contentful.[Warnings] ALTER COLUMN [TextId] NVARCHAR(30) NULL;
ALTER TABLE Contentful.[ComponentDropDowns] ALTER COLUMN [RichTextContentId] BIGINT NULL;
ALTER TABLE Contentful.[TextBodies] ALTER COLUMN [RichTextId] BIGINT NULL;
GO

CREATE UNIQUE INDEX IX_Sections_InterstitialPageId On Contentful.[Sections](InterstitialPageId) WHERE [InterstitialPageId] IS NOT NULL;
CREATE INDEX IX_Warnings_TextId On Contentful.[Warnings](TextId);
CREATE UNIQUE INDEX IX_ComponentDropDowns_RichTextContentId On Contentful.[ComponentDropDowns](RichTextContentId) WHERE [RichTextContentId] IS NOT NULL;
CREATE INDEX IX_TextBodies_RichTextId On Contentful.[TextBodies](RichTextId);
GO

ALTER TABLE Contentful.[Sections] ADD CONSTRAINT FK_Sections_Pages_InterstitialPageId FOREIGN KEY (InterstitialPageId) REFERENCES Contentful.Pages(Id);
ALTER TABLE Contentful.[Warnings] ADD CONSTRAINT FK_Warnings_TextBodies_TextId FOREIGN KEY (TextId) REFERENCES Contentful.TextBodies(Id);
ALTER TABLE Contentful.[ComponentDropDowns] ADD CONSTRAINT FK_ComponentDropDowns_RichTextContents_RichTextContentId FOREIGN KEY (RichTextContentId) REFERENCES Contentful.RichTextContents(Id) ON DELETE CASCADE;
ALTER TABLE Contentful.[TextBodies] ADD CONSTRAINT FK_TextBodies_RichTextContents_RichTextId FOREIGN KEY (RichTextId) REFERENCES Contentful.RichTextContents(Id) ON DELETE CASCADE;
GO
