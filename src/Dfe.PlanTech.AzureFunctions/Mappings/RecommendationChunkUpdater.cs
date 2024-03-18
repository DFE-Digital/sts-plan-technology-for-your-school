using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationChunkUpdater(ILogger<RecommendationChunkUpdater> logger, CmsDbContext db) : EntityUpdater(logger, db)
{

    public override async Task<MappedEntity> UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        var (incoming, existing) = MapToConcreteType<RecommendationChunkDbEntity>(entity);

        await AddOrUpdateRecommendationChunkAnswers(incoming, existing);
        await AddOrUpdateRecommendationChunkContents(incoming, existing);
        RemoveOldAssociatedChunkContents(incoming);
        RemoveOldAssociatedChunkAnswers(incoming);

        return entity;
    }

    private void RemoveOldAssociatedChunkAnswers(RecommendationChunkDbEntity incoming)
    {
        RemoveOldRelationships(incoming, GetRecommendationChunkAnswers, (RecommendationChunkAnswerDbEntity answer, RecommendationChunkDbEntity incoming) => incoming.Answers.Exists(incomingAnswer => answer.Matches(incoming, incomingAnswer)));
    }

    private void RemoveOldAssociatedChunkContents(RecommendationChunkDbEntity incoming)
    {
        RemoveOldRelationships(incoming, GetRecommendationChunkContents, (RecommendationChunkContentDbEntity content, RecommendationChunkDbEntity incoming) => incoming.Content.Exists(incomingContent => content.Matches(incoming, incomingContent)));
    }

    private async Task AddOrUpdateRecommendationChunkAnswers(RecommendationChunkDbEntity incoming, RecommendationChunkDbEntity existing)
    {
        static List<AnswerDbEntity> selectAnswers(RecommendationChunkDbEntity incoming) => incoming.Answers;
        static IQueryable<RecommendationChunkAnswerDbEntity> getMatchingRelationships(DbSet<RecommendationChunkAnswerDbEntity> dbSet, RecommendationChunkDbEntity incoming, AnswerDbEntity incomingAnswer)
        => dbSet.Where(chunkContent => chunkContent.RecommendationChunkId == incoming.Id && chunkContent.AnswerId == incomingAnswer.Id);

        await AddNewRelationshipsAndRemoveDuplicates(incoming, existing, GetRecommendationChunkAnswers, selectAnswers, getMatchingRelationships);
    }

    private async Task AddOrUpdateRecommendationChunkContents(RecommendationChunkDbEntity incoming, RecommendationChunkDbEntity existing)
    {
        static List<ContentComponentDbEntity> selectContent(RecommendationChunkDbEntity incoming) => incoming.Content;
        static IQueryable<RecommendationChunkContentDbEntity> getMatchingRelationships(DbSet<RecommendationChunkContentDbEntity> dbSet, RecommendationChunkDbEntity incoming, ContentComponentDbEntity incomingContent)
        => dbSet.Where(chunkContent => chunkContent.RecommendationChunkId == incoming.Id && chunkContent.ContentComponentId == incomingContent.Id);
        static void UpdateContentOrder(ContentComponentDbEntity content, RecommendationChunkContentDbEntity relationship)
        {
            if (relationship.ContentComponent!.Order != content.Order)
            {
                relationship.ContentComponent.Order = content.Order;
            }
        }

        await AddNewRelationshipsAndRemoveDuplicates(incoming, existing, GetRecommendationChunkContents, selectContent, getMatchingRelationships, UpdateContentOrder);
    }

    private static DbSet<RecommendationChunkAnswerDbEntity> GetRecommendationChunkAnswers(CmsDbContext db)
        => db.RecommendationChunkAnswers;

    private static DbSet<RecommendationChunkContentDbEntity> GetRecommendationChunkContents(CmsDbContext db)
        => db.RecommendationChunkContents;
}