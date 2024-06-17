DELETE
FROM Contentful.ContentComponents
WHERE Id in (
    Select Id From Contentful.RecommendationPages)

TRUNCATE TABLE Contentful.RecommendationPages

DROP TABLE Contentful.RecommendationPages

GO
