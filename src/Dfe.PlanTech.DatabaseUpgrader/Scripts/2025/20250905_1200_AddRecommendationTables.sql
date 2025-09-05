CREATE TABLE dbo.recommendation(
    id                 INT IDENTITY(1, 1) PRIMARY KEY,
    contentfulRef      NVARCHAR(50)  NOT NULL,
    dateCreated        DATETIME      NOT NULL,
    recommendationText NVARCHAR(MAX) NOT NULL,
    questionId         INT           NOT NULL,
    archived           BIT           NOT NULL
);
GO

-- FK
ALTER TABLE dbo.recommendation
    ADD CONSTRAINT FK_recommendation_question
        FOREIGN KEY (questionId) REFERENCES dbo.[question](id);
GO

CREATE TABLE dbo.establishmentRecommendationHistory(
    establishmentRecommendationId INT IDENTITY(1, 1) PRIMARY KEY,
    dateCreated                   DATETIME      NOT NULL,
    establishmentId               INT           NOT NULL,
    matEstablishmentId            INT           NULL,
    recommendationId              INT           NOT NULL,
    userId                        INT           NOT NULL,
    previousStatus                NVARCHAR(50)  NULL,
    currentStatus                 NVARCHAR(50)  NOT NULL,
    noteText                      NVARCHAR(MAX) NULL
);
GO

-- FK
ALTER TABLE dbo.establishmentRecommendationHistory
    ADD CONSTRAINT FK_erh_establishment
        FOREIGN KEY (establishmentId) REFERENCES dbo.establishment(id);

ALTER TABLE dbo.establishmentRecommendationHistory
    ADD CONSTRAINT FK_erh_matEstablishment
        FOREIGN KEY (matEstablishmentId) REFERENCES dbo.establishment(id);

ALTER TABLE dbo.establishmentRecommendationHistory
    ADD CONSTRAINT FK_erh_recommendation
        FOREIGN KEY (recommendationId) REFERENCES dbo.recommendation(id);

ALTER TABLE dbo.establishmentRecommendationHistory
    ADD CONSTRAINT FK_erh_user
        FOREIGN KEY (userId) REFERENCES dbo.[user](id);
GO
