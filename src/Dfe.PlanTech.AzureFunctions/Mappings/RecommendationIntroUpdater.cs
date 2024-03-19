using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationIntroUpdater(ILogger<RecommendationIntroUpdater> logger, CmsDbContext db) :EntityUpdater(logger, db)
{
    
    public override MappedEntity UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        if (entity.ExistingEntity is not RecommendationIntroDbEntity existingRecommendationIntro)
        {
            throw new InvalidCastException($"Entity is not the expected type. {entity.ExistingEntity!.GetType()}");
        }
        
        RemoveOldAssociatedIntroContents(existingRecommendationIntro);

        return entity;
    }
    
    private void RemoveOldAssociatedIntroContents(RecommendationIntroDbEntity existingRecommendationIntro)
    {
        var contentsToRemove = Db.RecommendationIntroContents
            .Where(content => content.RecommendationIntroId == existingRecommendationIntro.Id)
            .ToList();

        Db.RecommendationIntroContents.RemoveRange(contentsToRemove);
    }
    
}