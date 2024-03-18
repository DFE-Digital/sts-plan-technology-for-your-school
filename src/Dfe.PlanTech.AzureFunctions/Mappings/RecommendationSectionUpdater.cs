using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationSectionUpdater(ILogger<RecommendationSectionUpdater> logger, CmsDbContext db) : EntityUpdater(logger, db)
{
    public override async Task<MappedEntity> UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        var (incoming, existing) = MapToConcreteType<RecommendationSectionDbEntity>(entity);

        await AddOrUpdateRecommendationSectionAnswers(incoming, existing);
        await AddOrUpdateRecommendationSectionChunks(incoming, existing);
        RemoveOldAssociatedRecommendationChunks(incoming);
        RemoveOldAssociatedRecommendationAnswers(incoming);

        return entity;
    }

    private async Task AddOrUpdateRecommendationSectionAnswers(RecommendationSectionDbEntity incoming, RecommendationSectionDbEntity existing)
    {
        static List<AnswerDbEntity> selectAnswers(RecommendationSectionDbEntity incoming) => incoming.Answers;
        static bool answerMatches(RecommendationSectionDbEntity incoming, AnswerDbEntity answer, RecommendationSectionAnswerDbEntity sectionAnswer) => sectionAnswer.Matches(incoming, answer);

        await AddNewRelationshipsAndRemoveDuplicates<RecommendationSectionDbEntity, AnswerDbEntity, RecommendationSectionAnswerDbEntity>(incoming, existing, selectAnswers, answerMatches);
    }


    private async Task AddOrUpdateRecommendationSectionChunks(RecommendationSectionDbEntity incoming, RecommendationSectionDbEntity existing)
    {
        static List<RecommendationChunkDbEntity> selectChunks(RecommendationSectionDbEntity incoming) => incoming.Chunks;
        static bool chunkMatches(RecommendationSectionDbEntity incoming, RecommendationChunkDbEntity chunk, RecommendationSectionChunkDbEntity sectionChunk) => sectionChunk.Matches(incoming, chunk);

        await AddNewRelationshipsAndRemoveDuplicates<RecommendationSectionDbEntity, RecommendationChunkDbEntity, RecommendationSectionChunkDbEntity>(incoming, existing, selectChunks, chunkMatches);
    }

    private void RemoveOldAssociatedRecommendationChunks(RecommendationSectionDbEntity incomingSection)
    {
        static bool ChunkExistsAlready(RecommendationSectionChunkDbEntity sectionChunk, RecommendationSectionDbEntity incoming)
            => incoming.Chunks.Exists(incomingChunk => sectionChunk.Matches(incoming, incomingChunk));

        RemoveOldRelationships<RecommendationSectionDbEntity, RecommendationSectionChunkDbEntity>(incomingSection, ChunkExistsAlready);
    }

    private void RemoveOldAssociatedRecommendationAnswers(RecommendationSectionDbEntity incomingSection)
    {
        static bool AnswerExistsAlready(RecommendationSectionAnswerDbEntity sectionAnswer, RecommendationSectionDbEntity incoming)
            => incoming.Answers.Exists(incomingAnswer => sectionAnswer.Matches(incoming, incomingAnswer));

        RemoveOldRelationships<RecommendationSectionDbEntity, RecommendationSectionAnswerDbEntity>(incomingSection, AnswerExistsAlready);
    }
}