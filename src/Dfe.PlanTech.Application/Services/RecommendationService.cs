using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Services;

public class RecommendationService(
    IRecommendationWorkflow recommendationWorkflow
) : IRecommendationService
{
    private readonly IRecommendationWorkflow _recommendationWorkflow = recommendationWorkflow ?? throw new ArgumentNullException(nameof(recommendationWorkflow));

    public async Task<IEnumerable<SqlRecommendationDto>> UpsertRecommendations(IEnumerable<RecommendationModel> recommendationModels)
    {
        var contentfulSysIds = recommendationModels.Select(rm => rm.ContentfulSysId);
        var existingRecommendations = await _recommendationWorkflow.GetRecommendationsByContentfulReferencesAsync(contentfulSysIds);

        var existingRecommendationContentfulRefs = existingRecommendations.Select(r => r.ContentfulSysId).ToList();

        var recommendationsToUpdate = recommendationModels
            .Where(rm => !existingRecommendationContentfulRefs.Contains(rm.ContentfulSysId))
            .ToList();

        foreach(var existingRecommendation in existingRecommendations)
        {
            var recommendationModel = recommendationModels.FirstOrDefault(rm => rm.ContentfulSysId == existingRecommendation.ContentfulSysId);
            if (recommendationModel == null)
            {
                continue;
            }

            if (!string.Equals(recommendationModel.Text, existingRecommendation.RecommendationText))
            {
                recommendationsToUpdate.Add(recommendationModel);
            }
        }

        // Insert new recommendations
        throw new NotImplementedException();
    }
}
