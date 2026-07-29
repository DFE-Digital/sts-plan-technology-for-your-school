Update establishmentRecommendationHistory Set responseId = response.id
From response
Where establishmentRecommendationHistory.userId = response.userId;
