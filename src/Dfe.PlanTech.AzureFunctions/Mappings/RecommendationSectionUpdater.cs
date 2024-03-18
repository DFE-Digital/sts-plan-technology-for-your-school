using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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
        static IQueryable<RecommendationSectionAnswerDbEntity> getMatchingRelationships(DbSet<RecommendationSectionAnswerDbEntity> dbSet, RecommendationSectionDbEntity incoming, AnswerDbEntity incomingAnswer)
        => dbSet.Where(sectionAnswer => sectionAnswer.RecommendationSectionId == incoming.Id && sectionAnswer.AnswerId == incomingAnswer.Id);

        await AddNewRelationshipsAndRemoveDuplicates(incoming, existing, (db) => db.RecommendationSectionAnswers, selectAnswers, getMatchingRelationships);
    }


    private async Task AddOrUpdateRecommendationSectionChunks(RecommendationSectionDbEntity incoming, RecommendationSectionDbEntity existing)
    {
        static List<RecommendationChunkDbEntity> selectChunks(RecommendationSectionDbEntity incoming) => incoming.Chunks;
        static IQueryable<RecommendationSectionChunkDbEntity> getMatchingRelationships(DbSet<RecommendationSectionChunkDbEntity> dbSet, RecommendationSectionDbEntity incoming, RecommendationChunkDbEntity incomingChunk)
        => dbSet.Where(sectionChunk => sectionChunk.RecommendationSectionId == incoming.Id && sectionChunk.RecommendationChunkId == incomingChunk.Id);

        await AddNewRelationshipsAndRemoveDuplicates(incoming, existing, GetRecommendationSectionChunksDbSet, selectChunks, getMatchingRelationships);
    }

    private void RemoveOldAssociatedRecommendationChunks(RecommendationSectionDbEntity incomingSection)
    {
        static bool ChunkExistsAlready(RecommendationSectionChunkDbEntity sectionChunk, RecommendationSectionDbEntity incoming)
            => incoming.Chunks.Exists(incomingChunk => sectionChunk.Matches(incoming, incomingChunk));

        RemoveOldRelationships(incomingSection, GetRecommendationSectionChunksDbSet, ChunkExistsAlready);
    }

    private void RemoveOldAssociatedRecommendationAnswers(RecommendationSectionDbEntity incomingSection)
    {
        static bool AnswerExistsAlready(RecommendationSectionAnswerDbEntity sectionAnswer, RecommendationSectionDbEntity incoming)
            => incoming.Answers.Exists(incomingAnswer => sectionAnswer.Matches(incoming, incomingAnswer));

        RemoveOldRelationships(incomingSection, GetrecommendationSectionAnswersDbSet, AnswerExistsAlready);
    }

    private static DbSet<RecommendationSectionChunkDbEntity> GetRecommendationSectionChunksDbSet(CmsDbContext db)
        => db.RecommendationSectionChunks;

    private static DbSet<RecommendationSectionAnswerDbEntity> GetrecommendationSectionAnswersDbSet(CmsDbContext db)
        => db.RecommendationSectionAnswers;
}