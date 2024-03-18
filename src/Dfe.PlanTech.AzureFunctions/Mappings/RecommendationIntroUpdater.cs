using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationIntroUpdater(ILogger<RecommendationIntroUpdater> logger, CmsDbContext db) : EntityUpdater(logger, db)
{

    public override async Task<MappedEntity> UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        var (incoming, existing) = MapToConcreteType<RecommendationIntroDbEntity>(entity);

        await AddOrUpdateRecommendationIntroContents(incoming, existing);
        RemoveOldAssociatedIntroContents(existing);

        return entity;
    }

    private void RemoveOldAssociatedIntroContents(RecommendationIntroDbEntity incoming)
    {
        static bool ContentExistsAlready(RecommendationIntroContentDbEntity introContent, RecommendationIntroDbEntity incoming)
            => incoming.Content.Exists(incomingContent => introContent.Matches(incoming, incomingContent));

        RemoveOldRelationships(incoming, GetRecommendationIntroContents, ContentExistsAlready);
    }

    private async Task AddOrUpdateRecommendationIntroContents(RecommendationIntroDbEntity incoming, RecommendationIntroDbEntity existing)
    {
        static List<ContentComponentDbEntity> selectContent(RecommendationIntroDbEntity incoming) => incoming.Content;

        static IQueryable<RecommendationIntroContentDbEntity> getMatchingRelationships(DbSet<RecommendationIntroContentDbEntity> dbSet, RecommendationIntroDbEntity incoming, ContentComponentDbEntity incomingContent)
        => dbSet.Where(introContent => introContent.RecommendationIntroId == incoming.Id && introContent.ContentComponentId == incomingContent.Id);

        static void UpdateContentOrder(ContentComponentDbEntity content, RecommendationIntroContentDbEntity relationship)
        {
            if (relationship.ContentComponent!.Order != content.Order)
            {
                relationship.ContentComponent.Order = content.Order;
            }
        }

        await AddNewRelationshipsAndRemoveDuplicates(incoming, existing, GetRecommendationIntroContents, selectContent, getMatchingRelationships, UpdateContentOrder);
    }

    private static DbSet<RecommendationIntroContentDbEntity> GetRecommendationIntroContents(CmsDbContext db)
        => db.RecommendationIntroContents;

}