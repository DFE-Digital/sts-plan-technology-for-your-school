using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class RecommendationRepository : IRecommendationRepository
{
    public async Task<IEnumerable<RecommendationEntity>> GetRecommendationsByContentfulReferencesAsync(IEnumerable<string> recommendationContentfulReferences)
    {
        /*
        // recommendation:
        10,16pZJdYnWP8q0SKwHfIfhh,2025-10-06 17:13:32.350,Upgrade to Wi-Fi 6,42309,false
        11,1bTRpHuW7BfCEJdmBExP59,2025-10-06 17:13:32.353,Identify problem areas in your school's Wi-Fi coverage,42310,false
        12,5jUKK2GLaRS1fXgdNyy5UC,2025-10-06 17:13:32.353,Plan for upgrading Wi-Fi when needed,42311,false
        13,62QL45tjop1sDoMxv1d9RK,2025-10-06 17:13:32.353,Make sure your Wi-Fi is suitable for use in schools,42312,false
        14,5QS1BHYDoAE4frKTtOVBQx,2025-10-06 17:13:32.353,Keep your school's Wi-Fi under warranty,42313,false
        15,4z2881vta9YxDCZT9MIZ0N,2025-10-06 17:13:32.353,Make sure your Wi-Fi automatically receives software updates,42314,false
        16,2mJKlrXHuqtFjQoOMvFx71,2025-10-06 17:13:32.353,Set up secure guest Wi-Fi,42315,false
        17,49mqHGocWo4Ot91454835a,2025-10-06 17:13:32.353,Manage your school's Wi-Fi through a central platform,42316,false
        18,5iMQrbQib3kiZELZUe0feh,2025-10-06 17:13:32.353,Review your Wi-Fi every year,42237,false
        */

        // return [];

        var stubData = new List<RecommendationEntity>
        {
            new RecommendationEntity
            {
                Id = 10,
                ContentfulRef = "16pZJdYnWP8q0SKwHfIfhh",
                DateCreated = DateTime.Parse("2025-10-06 17:13:32.350"),
                RecommendationText = "Upgrade to Wi-Fi 6",
                QuestionId = 42309,
                Archived = false,
                Question = null
            },
            new RecommendationEntity
            {
                Id = 11,
                ContentfulRef = "1bTRpHuW7BfCEJdmBExP59",
                DateCreated = DateTime.Parse("2025-10-06 17:13:32.353"),
                RecommendationText = "Identify problem areas in your school's Wi-Fi coverage",
                QuestionId = 42310,
                Archived = false,
                Question = null
            },
            new RecommendationEntity {
                Id = 12,
                ContentfulRef = "5jUKK2GLaRS1fXgdNyy5UC",
                DateCreated = DateTime.Parse("2025-10-06 17:13:32.353"),
                RecommendationText = "Plan for upgrading Wi-Fi when needed",
                QuestionId = 42311,
                Archived = false,
                Question = null
            },
            new RecommendationEntity()
            {
                Id = 13,
                ContentfulRef = "62QL45tjop1sDoMxv1d9RK",
                DateCreated = DateTime.Parse("2025-10-06 17:13:32.353"),
                RecommendationText = "Make sure your Wi-Fi is suitable for use in schools",
                QuestionId = 42312,
                Archived = false,
                Question = null
            },
            new()
            {
                Id = 14,
                ContentfulRef = "5QS1BHYDoAE4frKTtOVBQx",
                DateCreated = DateTime.Parse("2025-10-06 17:13:32.353"),
                RecommendationText = "Keep your school's Wi-Fi under warranty",
                QuestionId = 42313,
                Archived = false,
                Question = null
            },
            new()
            {
                Id = 15,
                ContentfulRef = "4z2881vta9YxDCZT9MIZ0N",
                DateCreated = DateTime.Parse("2025-10-06 17:13:32.353"),
                RecommendationText = "Make sure your Wi-Fi automatically receives software updates",
                QuestionId = 42314,
                Archived = false,
                Question = null
            },
            new()
            {
                Id = 16,
                ContentfulRef = "2mJKlrXHuqtFjQoOMvFx71",
                DateCreated = DateTime.Parse("2025-10-06 17:13:32.353"),
                RecommendationText = "Set up secure guest Wi-Fi",
                QuestionId = 42315,
                Archived = false,
                Question = null
            },
            new()
            {
                Id = 17,
                ContentfulRef = "49mqHGocWo4Ot91454835a",
                DateCreated = DateTime.Parse("2025-10-06 17:13:32.353"),
                RecommendationText = "Manage your school's Wi-Fi through a central platform",
                QuestionId = 42316,
                Archived = false,
                Question = null
            },
            new()
            {
                Id = 18,
                ContentfulRef = "5iMQrbQib3kiZELZUe0feh",
                DateCreated = DateTime.Parse("2025-10-06 17:13:32.353"),
                RecommendationText = "Review your Wi-Fi every year",
                QuestionId = 42237,
                Archived = false,
                Question = null
            }
        };

        return stubData.Where(r => recommendationContentfulReferences.Contains(r.ContentfulRef));
    }
}
