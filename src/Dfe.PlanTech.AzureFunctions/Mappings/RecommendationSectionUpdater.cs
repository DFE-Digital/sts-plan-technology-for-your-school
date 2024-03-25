using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationSectionUpdater(ILogger<RecommendationSectionUpdater> logger, CmsDbContext db) : EntityUpdater(logger, db)
{
    public override MappedEntity UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        if (entity.ExistingEntity is not RecommendationSectionDbEntity existingRecommendationSection)
        {
            throw new InvalidCastException($"Entity is not the expected type. {entity.ExistingEntity!.GetType()}");
        }

        RemoveOldAssociatedRecommendationChunks(existingRecommendationSection);
        RemoveOldAssociatedRecommendationAnswers(existingRecommendationSection);


        return entity;
    }


    private void RemoveOldAssociatedRecommendationChunks(RecommendationSectionDbEntity existingRecommendationSection)
    {
        var chunksToRemove = Db.RecommendationSectionChunks
            .Where(content => content.RecommendationSectionId == existingRecommendationSection.Id);

        Db.RecommendationSectionChunks.RemoveRange(chunksToRemove);
    }

    private void RemoveOldAssociatedRecommendationAnswers(RecommendationSectionDbEntity existingRecommendationSection)
    {
        var chunksToRemove = Db.RecommendationSectionAnswers
            .Where(content => content.RecommendationSectionId == existingRecommendationSection.Id);

        Db.RecommendationSectionAnswers.RemoveRange(chunksToRemove);
    }


}