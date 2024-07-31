IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'RecommendationChunks' 
               AND TABLE_SCHEMA = 'Contentful' 
               AND COLUMN_NAME = 'CSLinkText')
BEGIN
    ALTER TABLE Contentful.RecommendationChunks
        ADD [CSLinkText] NVARCHAR(256);
END;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'RecommendationChunks' 
               AND TABLE_SCHEMA = 'Contentful' 
               AND COLUMN_NAME = 'CSUrl')
BEGIN
    ALTER TABLE Contentful.RecommendationChunks
        ADD [CSUrl] NVARCHAR(256);
END;