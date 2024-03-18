using System.Linq.Expressions;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class SubtopicRecommendationUpdater(ILogger<SubtopicRecommendationUpdater> logger, CmsDbContext db) : EntityUpdater(logger, db)
{
    public override async Task<MappedEntity> UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        var (incoming, existing) = MapToConcreteType<SubtopicRecommendationDbEntity>(entity);

        await AddOrRemoveSubtopicRecommendationIntros(incoming, existing);
        RemoveOldRemovedIntros(incoming);

        return entity;
    }

    private async Task AddOrRemoveSubtopicRecommendationIntros(SubtopicRecommendationDbEntity incoming, SubtopicRecommendationDbEntity existing)
    {
        static List<RecommendationIntroDbEntity> selectIntros(SubtopicRecommendationDbEntity incoming) => incoming.Intros;
        static IQueryable<SubtopicRecommendationIntroDbEntity> getMatchingRelationships(DbSet<SubtopicRecommendationIntroDbEntity> dbSet, SubtopicRecommendationDbEntity incoming, RecommendationIntroDbEntity incomingIntro)
        => dbSet.Where(subtopicRec => subtopicRec.SubtopicRecommendationId == incoming.Id && subtopicRec.RecommendationIntroId == incomingIntro.Id);

        await AddNewRelationshipsAndRemoveDuplicates(incoming, existing, GetSubtopicRecommendationIntros, selectIntros, getMatchingRelationships);
    }

    private void RemoveOldRemovedIntros(SubtopicRecommendationDbEntity incoming)
    {
        static bool introAlreadyExists(SubtopicRecommendationIntroDbEntity recommendationIntro, SubtopicRecommendationDbEntity incoming)
            => incoming.RecommendationIntro.Exists(incomingChunk => recommendationIntro.Matches(incoming, incomingChunk));

        RemoveOldRelationships(incoming, GetSubtopicRecommendationIntros, introAlreadyExists);
    }

    private static DbSet<SubtopicRecommendationIntroDbEntity> GetSubtopicRecommendationIntros(CmsDbContext db)
    => db.SubtopicRecommendationIntros;

}