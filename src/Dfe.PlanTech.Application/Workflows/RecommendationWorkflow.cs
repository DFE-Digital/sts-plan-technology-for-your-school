using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Application.Workflows;

public class RecommendationWorkflow(
    IRecommendationRepository recommendationRepository
) : IRecommendationWorkflow
{
    private readonly IRecommendationRepository _recommendationRepository = recommendationRepository ?? throw new ArgumentNullException(nameof(recommendationRepository));

    public async Task<IEnumerable<SqlRecommendationDto>> GetRecommendationsByContentfulReferencesAsync(IEnumerable<string> contentfulRefs)
    {
        var recommendations = await _recommendationRepository.GetRecommendationsByContentfulReferencesAsync(contentfulRefs);
        return recommendations.Select(r => r.AsDto());
    }
}
