UPDATE dbo.establishmentRecommendationHistory
SET matEstablishmentId = NULL
WHERE establishmentId = matEstablishmentId;
GO
