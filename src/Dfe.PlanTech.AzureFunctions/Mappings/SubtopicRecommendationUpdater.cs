using System.Linq.Expressions;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

        var (incoming, existing) = MapToConcreteType<SubtopicRecommendationDbEntity>(entity);

        AddOrRemoveSubtopicRecommendationIntros(incoming, existing);
        RemoveOldRemovedIntros(incoming, existing);

        return entity;
    }

    private static void AddOrRemoveSubtopicRecommendationIntros(SubtopicRecommendationDbEntity incoming, SubtopicRecommendationDbEntity existing)
    {
        static List<RecommendationIntroDbEntity> selectIntros(SubtopicRecommendationDbEntity incoming) => incoming.Intros;

        AddNewRelationshipsAndRemoveDuplicates<SubtopicRecommendationDbEntity, RecommendationIntroDbEntity, string>(incoming, existing, selectIntros);
    }

    private static void RemoveOldRemovedIntros(SubtopicRecommendationDbEntity incoming, SubtopicRecommendationDbEntity existing)
        => existing.Intros.RemoveAll(existingIntro => !incoming.Intros.Exists(incomingIntro => existing.Id == incomingIntro.Id));
}