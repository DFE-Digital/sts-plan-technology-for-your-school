using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationIntroUpdater(ILogger<RecommendationIntroUpdater> logger, CmsDbContext db) : EntityUpdater(logger, db)
{

    public override MappedEntity UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        var (incoming, existing) = MapToConcreteType<RecommendationIntroDbEntity>(entity);

        AddOrUpdateRecommendationIntroContents(incoming, existing);
        RemoveOldRemovedIntros(incoming, existing);

        return entity;
    }

    private static void RemoveOldRemovedIntros(RecommendationIntroDbEntity incoming, RecommendationIntroDbEntity existing)
        => existing.Content.RemoveAll(existingContent => !incoming.Content.Exists(incomingContent => existing.Id == incomingContent.Id));

    private static void AddOrUpdateRecommendationIntroContents(RecommendationIntroDbEntity incoming, RecommendationIntroDbEntity existing)
    {
        static List<ContentComponentDbEntity> selectContent(RecommendationIntroDbEntity incoming) => incoming.Content;

        static void UpdateContentOrder(ContentComponentDbEntity incoming, ContentComponentDbEntity existing)
        {
            if (existing.Order != incoming.Order)
            {
                existing.Order = incoming.Order;
            }
        }

        AddNewRelationshipsAndRemoveDuplicates(incoming, existing, selectContent, UpdateContentOrder);
    }
}