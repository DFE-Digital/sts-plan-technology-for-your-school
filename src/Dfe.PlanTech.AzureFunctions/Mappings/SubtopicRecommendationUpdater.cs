using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
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
        static bool answerMatches(SubtopicRecommendationDbEntity incoming, RecommendationIntroDbEntity answer, SubtopicRecommendationIntroDbEntity subtopicRecommendationIntro) => subtopicRecommendationIntro.Matches(incoming, answer);

        await AddNewRelationshipsAndRemoveDuplicates<SubtopicRecommendationDbEntity, RecommendationIntroDbEntity, SubtopicRecommendationIntroDbEntity>(incoming, existing, selectIntros, answerMatches);
    }

    private void RemoveOldRemovedIntros(SubtopicRecommendationDbEntity incoming)
    {
        static bool introAlreadyExists(SubtopicRecommendationIntroDbEntity recommendationIntro, SubtopicRecommendationDbEntity incoming)
            => incoming.RecommendationIntro.Exists(incomingChunk => recommendationIntro.Matches(incoming, incomingChunk));

        RemoveOldRelationships<SubtopicRecommendationDbEntity, SubtopicRecommendationIntroDbEntity>(incoming, introAlreadyExists);
    }
}