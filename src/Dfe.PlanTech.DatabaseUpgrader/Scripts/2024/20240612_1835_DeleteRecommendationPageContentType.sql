IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'Contentful' AND TABLE_NAME = 'RecommendationPages')

BEGIN
    DELETE
    FROM Contentful.ContentComponents
    WHERE Id in (
        Select Id From Contentful.RecommendationPages)

    TRUNCATE TABLE Contentful.RecommendationPages

    DROP TABLE Contentful.RecommendationPages
END
