-- Add missing foreign keys from Component tables to the ContentComponents table

ALTER TABLE Contentful.Buttons
    ADD CONSTRAINT FK_Buttons_ContentComponents_Id
        FOREIGN KEY (Id) REFERENCES Contentful.ContentComponents
            ON DELETE CASCADE

ALTER TABLE Contentful.ButtonWithEntryReferences
    ADD CONSTRAINT FK_ButtonsWithEntryReferences_ContentComponents_Id
        FOREIGN KEY (Id) REFERENCES Contentful.ContentComponents
            ON DELETE CASCADE

ALTER TABLE Contentful.ButtonWithLinks
    ADD CONSTRAINT FK_ButtonsWithLinks_ContentComponents_Id
        FOREIGN KEY (Id) REFERENCES Contentful.ContentComponents
            ON DELETE CASCADE

ALTER TABLE Contentful.ComponentDropDowns
    ADD CONSTRAINT FK_ComponentDropDowns_ContentComponents_Id
        FOREIGN KEY (Id) REFERENCES Contentful.ContentComponents
            ON DELETE CASCADE

ALTER TABLE Contentful.Headers
    ADD CONSTRAINT FK_Headers_ContentComponents_Id
        FOREIGN KEY (Id) REFERENCES Contentful.ContentComponents
            ON DELETE CASCADE

ALTER TABLE Contentful.InsetTexts
    ADD CONSTRAINT FK_InsetTexts_ContentComponents_Id
        FOREIGN KEY (Id) REFERENCES Contentful.ContentComponents
            ON DELETE CASCADE

ALTER TABLE Contentful.NavigationLink
    ADD CONSTRAINT FK_NavigationLink_ContentComponents_Id
        FOREIGN KEY (Id) REFERENCES Contentful.ContentComponents
            ON DELETE CASCADE

ALTER TABLE Contentful.RecommendationIntros
    ADD CONSTRAINT FK_RecommendationIntros_ContentComponents_Id
        FOREIGN KEY (Id) REFERENCES Contentful.ContentComponents
            ON DELETE CASCADE

GO

-- Add a dateCreated and dateLastUpdated column to the ContentComponents table

ALTER TABLE Contentful.ContentComponents
    ADD dateCreated DATETIME DEFAULT GETUTCDATE(),
        dateLastUpdated DATETIME DEFAULT GETUTCDATE()

GO

-- Add a trigger to the ContentfulComponents table to automatically update dateLastUpdated whenever a record is updated

CREATE TRIGGER Contentful.tr_ContentComponents
    ON Contentful.ContentComponents
    FOR INSERT, UPDATE
    AS
BEGIN
    UPDATE CC
    SET dateLastUpdated = GETUTCDATE()
    FROM ContentComponents CC
    JOIN inserted I ON I.id = CC.id
END

GO

-- Add internal name column to every primary content component table
-- i.e. every table in the Contentful Schema that has an FK from its PK to the ContentComponent table
-- Except Categories and Pages which already have them

ALTER TABLE Contentful.TextBodies ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.Sections ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.Warnings ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.Questions ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.Answers ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.RecommendationChunks ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.RecommendationIntros ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.CSLinks ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.Buttons ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.Headers ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.InsetTexts ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.NavigationLink ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.ButtonWithLinks ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.ButtonWithEntryReferences ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.Titles ADD InternalName NVARCHAR(256)
ALTER TABLE Contentful.ComponentDropDowns ADD InternalName NVARCHAR(256)
GO

-- Checking InternalName is null is redundant but it stops sqlcheck deeming this an unsafe query 'update with no where'

UPDATE Contentful.TextBodies SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.Sections SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.Warnings SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.Questions SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.Answers SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.RecommendationChunks SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.RecommendationIntros SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.CSLinks SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.Buttons SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.Headers SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.InsetTexts SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.NavigationLink SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.ButtonWithLinks SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.ButtonWithEntryReferences SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.Titles SET InternalName = '' WHERE InternalName IS NULL
UPDATE Contentful.ComponentDropDowns SET InternalName = '' WHERE InternalName IS NULL
GO

ALTER TABLE Contentful.TextBodies ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Sections ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Warnings ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Questions ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Answers ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.RecommendationChunks ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.RecommendationIntros ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.CSLinks ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Buttons ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Headers ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.InsetTexts ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.NavigationLink ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.ButtonWithLinks ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.ButtonWithEntryReferences ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Titles ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.ComponentDropDowns ALTER COLUMN InternalName NVARCHAR(256) NOT NULL
GO


-- Alter short text NVARCHAR(MAX) columns to be NVARCHAR(256)

ALTER TABLE Contentful.Sections ALTER COLUMN [Name] NVARCHAR(256)
ALTER TABLE Contentful.Questions ALTER COLUMN [Text] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Questions ALTER COLUMN [HelpText] NVARCHAR(256)
ALTER TABLE Contentful.Questions ALTER COLUMN [Slug] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.RecommendationChunks ALTER COLUMN [Header] NVARCHAR(256)
ALTER TABLE Contentful.Answers ALTER COLUMN [Text] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Answers ALTER COLUMN [Maturity] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.RecommendationIntros ALTER COLUMN [Slug] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Buttons ALTER COLUMN [Value] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Headers ALTER COLUMN [Text] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.InsetTexts ALTER COLUMN [Text] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.NavigationLink ALTER COLUMN [DisplayText] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.NavigationLink ALTER COLUMN [Href] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.ButtonWithLinks ALTER COLUMN [Href] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Titles ALTER COLUMN [Text] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Categories ALTER COLUMN [InternalName] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Pages ALTER COLUMN [InternalName] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.Pages ALTER COLUMN [Slug] NVARCHAR(256) NOT NULL
ALTER TABLE Contentful.ComponentDropDowns ALTER COLUMN [Title] NVARCHAR(256) NOT NULL
GO
