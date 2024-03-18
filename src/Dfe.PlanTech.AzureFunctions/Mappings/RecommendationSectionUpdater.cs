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

        var (incoming, existing) = MapToConcreteType<RecommendationSectionDbEntity>(entity);

        AddOrUpdateRecommendationSectionAnswers(incoming, existing);
        AddOrUpdateRecommendationSectionChunks(incoming, existing);
        RemoveOldAssociatedRecommendationChunks(incoming, existing);
        RemoveOldAssociatedRecommendationAnswers(incoming, existing);

        return entity;
    }

    private void AddOrUpdateRecommendationSectionAnswers(RecommendationSectionDbEntity incoming, RecommendationSectionDbEntity existing)
    {
        static List<AnswerDbEntity> selectAnswers(RecommendationSectionDbEntity entity) => entity.Answers;

        AddNewRelationshipsAndRemoveDuplicates<RecommendationSectionDbEntity, AnswerDbEntity, string>(incoming, existing, selectAnswers);
    }


    private void AddOrUpdateRecommendationSectionChunks(RecommendationSectionDbEntity incoming, RecommendationSectionDbEntity existing)
    {
        static List<RecommendationChunkDbEntity> selectChunks(RecommendationSectionDbEntity entity) => entity.Chunks;

        AddNewRelationshipsAndRemoveDuplicates<RecommendationSectionDbEntity, RecommendationChunkDbEntity, string>(incoming, existing, selectChunks);
    }

    private static void RemoveOldAssociatedRecommendationChunks(RecommendationSectionDbEntity incoming, RecommendationSectionDbEntity existing)
        => existing.Chunks.RemoveAll(existingChunk => !incoming.Chunks.Exists(incomingChunk => existing.Id == incomingChunk.Id));


    private static void RemoveOldAssociatedRecommendationAnswers(RecommendationSectionDbEntity incoming, RecommendationSectionDbEntity existing)
        => existing.Answers.RemoveAll(existingAnswer => !incoming.Answers.Exists(incomingAnswer => existing.Id == incomingAnswer.Id));
}