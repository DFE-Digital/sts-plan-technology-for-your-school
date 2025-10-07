using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class RecommendationWorkflow(
    IEstablishmentRecommendationHistoryRepository establishmentRecommendationHistoryRepository,
    IRecommendationRepository recommendationRepository
) : IRecommendationWorkflow
{
    private readonly IEstablishmentRecommendationHistoryRepository _establishmentRecommendationHistoryRepository = establishmentRecommendationHistoryRepository ?? throw new ArgumentNullException(nameof(establishmentRecommendationHistoryRepository));
    private readonly IRecommendationRepository _recommendationRepository = recommendationRepository ?? throw new ArgumentNullException(nameof(recommendationRepository));

    public async Task<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>> GetMostRecentRecommendationStatusesAsync(
        IEnumerable<string> recommendationContentfulReferences,
        int establishmentId
    )
    {
        var recommendations = await _recommendationRepository.GetRecommendationIdsByContentfulReferencesAsync(recommendationContentfulReferences);
        var recommendationIdToContentfulReferenceDictionary = recommendations
            .ToDictionary(r => r.Id, r => r.ContentfulRef);

        var recommendationHistoryEntities = await _establishmentRecommendationHistoryRepository.GetRecommendationHistoryByEstablishmentIdAsync(establishmentId);

        return recommendationHistoryEntities
            .GroupBy(rhe => rhe.RecommendationId)
            .ToDictionary(
                group => recommendationIdToContentfulReferenceDictionary[group.Key],
                group => group.OrderByDescending(r => r.DateCreated).First().AsDto()
            );
    }

    public Task<SqlEstablishmentDto> GetOrCreateEstablishmentAsync(string establishmentUrn, string establishmentName)
    {
        var establishmentModel = new EstablishmentModel()
        {
            Name = establishmentName,
            Urn = establishmentUrn
        };

        return GetOrCreateEstablishmentAsync(establishmentModel);
    }

    public async Task<SqlEstablishmentDto?> GetEstablishmentByReferenceAsync(string establishmentReference)
    {
        var establishments = await _establishmentRepository.GetEstablishmentsByReferencesAsync([establishmentReference]);
        return establishments.FirstOrDefault()?.AsDto();
    }

    public async Task<IEnumerable<SqlEstablishmentDto>> GetEstablishmentsByReferencesAsync(IEnumerable<string> establishmentReferences)
    {
        var establishments = await _establishmentRepository.GetEstablishmentsByReferencesAsync(establishmentReferences);
        return establishments.Select(e => e.AsDto());
    }

    public async Task<List<SqlEstablishmentLinkDto>> GetGroupEstablishments(int establishmentId)
    {
        var links = await _establishmentLinkRepository.GetGroupEstablishmentsByEstablishmentIdAsync(establishmentId);
        return links.Select(l => l.AsDto()).ToList();
    }

    public Task<int> RecordGroupSelection(UserGroupSelectionModel userGroupSelectionModel)
    {
        return _storedProcedureRepository.RecordGroupSelection(userGroupSelectionModel);
    }
}
