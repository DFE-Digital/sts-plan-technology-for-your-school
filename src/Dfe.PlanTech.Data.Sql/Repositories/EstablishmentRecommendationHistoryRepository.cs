using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Repositories;

public class EstablishmentRecommendationHistoryRepository : IEstablishmentRecommendationHistoryRepository
{
    public async Task<IEnumerable<EstablishmentRecommendationHistoryEntity>> GetRecommendationHistoryByEstablishmentIdAsync(int establishmentId)
    {

        /*
         // establishment recommendation history:
        111,2025-10-07 19:53:37.323,16,,12,83,Completed,Completed
        112,2025-10-07 19:53:37.323,16,,13,83,Not Started,Not Started
        113,2025-10-07 19:53:37.323,16,,14,83,Not Started,Not Started
        114,2025-10-07 19:53:37.323,16,,15,83,Not Started,Not Started
        115,2025-10-07 19:53:37.323,16,,16,83,Completed,Completed
        116,2025-10-07 19:53:37.323,16,,17,83,Completed,Completed
        117,2025-10-07 19:53:37.323,16,,18,83,Completed,Completed
        110,2025-10-07 19:53:37.320,16,,11,83,Completed,Completed
        109,2025-10-07 19:53:37.290,16,,10,83,Not Started,Not Started
        */

        // return [];

        var stubData = new List<EstablishmentRecommendationHistoryEntity>
        {
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 111,
                EstablishmentId = establishmentId,
                RecommendationId = 12,
                UserId = 83,
                DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                PreviousStatus = "Completed",
                NewStatus = "Completed"
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 112,
                EstablishmentId = establishmentId,
                RecommendationId = 13,
                UserId = 83,
                DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                PreviousStatus = "Not Started",
                NewStatus = "Not Started"
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 113,
                EstablishmentId = establishmentId,
                RecommendationId = 14,
                UserId = 83,
                DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                PreviousStatus = "Not Started",
                NewStatus = "Not Started"
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 114,
                EstablishmentId = establishmentId,
                RecommendationId = 15,
                UserId = 83,
                DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                PreviousStatus = "Not Started",
                NewStatus = "Not Started"
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 115,
                EstablishmentId = establishmentId,
                RecommendationId = 16,
                UserId = 83,
                DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                PreviousStatus = "Completed",
                NewStatus = "Completed"
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 116,
                EstablishmentId = establishmentId,
                RecommendationId = 17,
                UserId = 83,
                DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                PreviousStatus = "Completed",
                NewStatus = "Completed"
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 117,
                EstablishmentId = establishmentId,
                RecommendationId = 18,
                UserId = 83,
                DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                PreviousStatus = "Completed",
                NewStatus = "Completed"
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 110,
                EstablishmentId = establishmentId,
                RecommendationId = 11,
                UserId = 83,
                DateCreated = DateTime.Parse("2025-10-07 19:53:37.320"),
                PreviousStatus = "Completed",
                NewStatus = "Completed"
            },
            new EstablishmentRecommendationHistoryEntity
            {
                Id = 109,
                EstablishmentId = establishmentId,
                RecommendationId = 10,
                UserId = 83,
                DateCreated = DateTime.Parse("2025-10-07 19:53:37.290"),
                PreviousStatus = "Not Started",
                NewStatus = "Not Started"
            },
        };

        return await Task.FromResult(stubData);

    }
}
