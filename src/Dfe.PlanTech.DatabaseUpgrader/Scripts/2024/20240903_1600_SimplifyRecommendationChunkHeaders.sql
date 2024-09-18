DROP TABLE IF EXISTS #RecChunks

SELECT
    RC.Id,
    RC.HeaderId,
    RC.CSLinkId,
    H.Text
INTO #RecChunks
FROM Contentful.RecommendationChunks RC
JOIN Contentful.Headers H ON RC.HeaderId = H.Id

ALTER TABLE Contentful.RecommendationChunks
    DROP CONSTRAINT FK_RecommendationChunks_Headers_HeaderId
ALTER TABLE Contentful.RecommendationChunks
    DROP COLUMN HeaderId
ALTER TABLE Contentful.RecommendationChunks
    ADD Header NVARCHAR(MAX)

UPDATE RC
SET RC.Header = Temp.Text
From #RecChunks Temp
Join Contentful.RecommendationChunks RC ON Temp.Id = RC.Id

Delete H
From Contentful.Headers H
Join #RecChunks RC ON RC.HeaderId = H.Id
