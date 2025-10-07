using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Workflows;

public class RecommendationWorkflow(
    // IEstablishmentRecommendationHistoryRepository establishmentRecommendationHistoryRepository,
    // IRecommendationRepository recommendationRepository
) : IRecommendationWorkflow
{
    public async Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetLatestRecommendationStatusesByRecommendationIdAsync(IEnumerable<string> recommendationContentfulReferences,
        int establishmentId)
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

        /*
public class SqlEstablishmentRecommendationHistoryDto : ISqlDto
{
    public int EstablishmentId { get; init; }
    public int RecommendationId { get; init; }
    public int UserId { get; init; }
    public int? MatEstablishmentId { get; init; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public string? PreviousStatus { get; init; }
    public string NewStatus { get; init; } = null!;
    public string? NoteText { get; init; }
}

         */


        // return [];

        var stubMockData = new Dictionary<string, SqlEstablishmentRecommendationHistoryDto>
        {
            {
                "16pZJdYnWP8q0SKwHfIfhh",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = establishmentId,
                    RecommendationId = 10,
                    UserId = 83,
                    DateCreated = DateTime.Parse("2025-10-07 19:53:37.290"),
                    PreviousStatus = "Not Started",
                    NewStatus = "Not Started",
                    NoteText = null
                }
            },
            {
                "1bTRpHuW7BfCEJdmBExP59",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = establishmentId,
                    RecommendationId = 11,
                    UserId = 83,
                    DateCreated = DateTime.Parse("2025-10-07 19:53:37.320"),
                    PreviousStatus = "Completed",
                    NewStatus = "Completed",
                    NoteText = null
                }
            },
            {
                "5jUKK2GLaRS1fXgdNyy5UC",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = establishmentId,
                    RecommendationId = 12,
                    UserId = 83,
                    DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                    PreviousStatus = "Completed",
                    NewStatus = "Completed",
                    NoteText = null
                }
            },
            {
                "62QL45tjop1sDoMxv1d9RK",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = establishmentId,
                    RecommendationId = 13,
                    UserId = 83,
                    DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                    PreviousStatus = "Not Started",
                    NewStatus = "Not Started",
                    NoteText = null
                }
            },
            {
                "5QS1BHYDoAE4frKTtOVBQx",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = establishmentId,
                    RecommendationId = 14,
                    UserId = 83,
                    DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                    PreviousStatus = "Not Started",
                    NewStatus = "Not Started",
                    NoteText = null
                }
            },
            {
                "4z2881vta9YxDCZT9MIZ0N",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = establishmentId,
                    RecommendationId = 15,
                    UserId = 83,
                    DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                    PreviousStatus = "Not Started",
                    NewStatus = "Not Started",
                    NoteText = null
                }
            },
            {
                "2mJKlrXHuqtFjQoOMvFx71",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = establishmentId,
                    RecommendationId = 16,
                    UserId = 83,
                    DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                    PreviousStatus = "Completed",
                    NewStatus = "Completed",
                    NoteText = null
                }
            },
            {
                "49mqHGocWo4Ot91454835a",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = establishmentId,
                    RecommendationId = 17,
                    UserId = 83,
                    DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                    PreviousStatus = "Completed",
                    NewStatus = "Completed",
                    NoteText = null
                }
            },
            {
                "5iMQrbQib3kiZELZUe0feh",
                new SqlEstablishmentRecommendationHistoryDto
                {
                    EstablishmentId = establishmentId,
                    RecommendationId = 18,
                    UserId = 83,
                    DateCreated = DateTime.Parse("2025-10-07 19:53:37.323"),
                    PreviousStatus = "Completed",
                    NewStatus = "Completed",
                    NoteText = null
                }
            }
        };

        return await Task.FromResult(stubMockData);



        // var recommendations = await recommendationRepository.GetRecommendationIdsByContentfulReferencesAsync(recommendationContentfulReferences);
        // var recommendationIdToContentfulReferenceDictionary = recommendations
        //     .ToDictionary(r => r.Id, r => r.ContentfulRef);
        //
        // var recommendationHistoryEntities = await establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId);
        //
        // return recommendationHistoryEntities
        //     .GroupBy(rhe => rhe.RecommendationId)
        //     .ToDictionary(
        //         group => recommendationIdToContentfulReferenceDictionary[group.Key],
        //         group => group.OrderByDescending(r => r.DateCreated).First().AsDto()
        //     );
    }
}
