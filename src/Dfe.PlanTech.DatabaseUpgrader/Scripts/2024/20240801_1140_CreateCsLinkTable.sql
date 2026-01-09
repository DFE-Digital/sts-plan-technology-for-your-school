IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [name] = 'CSLinks')
BEGIN
    CREATE TABLE Contentful.CSLinks
    (
        Id       NVARCHAR(30) NOT NULL
            CONSTRAINT PK_CSLinks PRIMARY KEY
            CONSTRAINT FK_CSLinks_ContentComponents_Id REFERENCES Contentful.ContentComponents(Id) ON DELETE CASCADE,

        LinkText NVARCHAR(256) NOT NULL,
        Url      NVARCHAR(256) NOT NULL
    );

    ALTER TABLE Contentful.RecommendationChunks
        ADD CSLinkId NVARCHAR(30);

    ALTER TABLE Contentful.RecommendationChunks
        ADD CONSTRAINT FK_RecommendationChunks_CSLinks_CSLinkId
            FOREIGN KEY (CSLinkId) REFERENCES Contentful.CSLinks(Id);
END;
