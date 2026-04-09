BEGIN
	ALTER TABLE dbo.establishmentRecommendationHistory
	ADD responseId INT NULL;

	CREATE INDEX IX_establishmentRecommendationHistory_responseId
	ON dbo.establishmentRecommendationHistory (responseId);

	ALTER TABLE dbo.establishmentRecommendationHistory
	ADD CONSTRAINT FK_establishmentRecommendationHistory_response_responseId
	FOREIGN KEY (responseId) REFERENCES dbo.[response](id)
	ON DELETE SET NULL;
END