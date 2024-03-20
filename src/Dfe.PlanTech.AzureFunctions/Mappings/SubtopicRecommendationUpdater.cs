using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class SubtopicRecommendationUpdater(ILogger<SubtopicRecommendationUpdater> logger, CmsDbContext db) : EntityUpdater(logger, db)
{
    public override MappedEntity UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        if (entity.ExistingEntity is not SubtopicRecommendationDbEntity existingSubtopicRecommendation)
        {
            throw new InvalidCastException($"Entity is not the expected type. {entity.ExistingEntity!.GetType()}");
        }

        RemoveOldAssociatedRecommendationIntros(existingSubtopicRecommendation);

        return entity;
    }

    private void RemoveOldAssociatedRecommendationIntros(SubtopicRecommendationDbEntity existingSubtopicRecommendation)
    {
        var introsToRemove = Db.SubtopicRecommendationIntros
            .Where(content => content.SubtopicRecommendationId == existingSubtopicRecommendation.Id)
            .ToList();

        Db.SubtopicRecommendationIntros.RemoveRange(introsToRemove);
    }
}