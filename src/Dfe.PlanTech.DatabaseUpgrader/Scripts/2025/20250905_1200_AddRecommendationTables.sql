CREATE TABLE dbo.recommendation(
    id                 INT IDENTITY(1, 1) NOT NULL,
    contentfulRef      NVARCHAR(50)  NOT NULL,
    dateCreated        DATETIME      NOT NULL,
    recommendationText NVARCHAR(MAX) NOT NULL,
    questionId         INT           NOT NULL,
    archived           BIT           NOT NULL,
    CONSTRAINT PK_recommendation PRIMARY KEY (id)
);
GO

-- FK
ALTER TABLE dbo.recommendation
    ADD CONSTRAINT FK_recommendation_question
        FOREIGN KEY (questionId) REFERENCES dbo.[question](id);
GO

CREATE TABLE dbo.establishmentRecommendationHistory(
    id                  INT IDENTITY(1,1) NOT NULL,
    dateCreated         DATETIME      NOT NULL,
    establishmentId     INT           NOT NULL,
    matEstablishmentId  INT           NULL,
    recommendationId    INT           NOT NULL,
    userId              INT           NOT NULL,
    previousStatus      NVARCHAR(50)  NULL,
    newStatus           NVARCHAR(50)  NOT NULL,
    noteText            NVARCHAR(MAX) NULL,
    CONSTRAINT PK_establishmentRecommendationHistory PRIMARY KEY (id)
);
GO

-- FK
ALTER TABLE dbo.establishmentRecommendationHistory
    ADD CONSTRAINT FK_erh_establishment
        FOREIGN KEY (establishmentId) REFERENCES dbo.establishment(id);
GO

ALTER TABLE dbo.establishmentRecommendationHistory
    ADD CONSTRAINT FK_erh_matEstablishment
        FOREIGN KEY (matEstablishmentId) REFERENCES dbo.establishment(id);
GO

ALTER TABLE dbo.establishmentRecommendationHistory
    ADD CONSTRAINT FK_erh_recommendation
        FOREIGN KEY (recommendationId) REFERENCES dbo.recommendation(id);
GO

ALTER TABLE dbo.establishmentRecommendationHistory
    ADD CONSTRAINT FK_erh_user
        FOREIGN KEY (userId) REFERENCES dbo.[user](id);
GO

-- Create indexes for performance on the append-only history table
CREATE INDEX IX_establishmentRecommendationHistory_EstablishmentRecommendation
    ON dbo.establishmentRecommendationHistory (establishmentId, recommendationId);
GO

CREATE INDEX IX_establishmentRecommendationHistory_DateCreated
    ON dbo.establishmentRecommendationHistory (dateCreated DESC);
GO
