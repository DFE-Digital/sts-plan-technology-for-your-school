using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationChunkUpdater(ILogger<RecommendationChunkUpdater> logger, CmsDbContext db) : EntityUpdater(logger, db)
{

    public override MappedEntity UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        if (entity.ExistingEntity is not RecommendationChunkDbEntity existingRecommendationChunk ||
            entity.IncomingEntity is not RecommendationChunkDbEntity incomingRecommendationChunk)
        {
            throw new InvalidCastException($"Entity is not the expected type. {entity.ExistingEntity!.GetType()}");
        }


        AddOrUpdateRecommendationChunkContents(incomingRecommendationChunk, existingRecommendationChunk); ;
        RemoveOldAssociatedChunkContents(incomingRecommendationChunk);
        RemoveOldAssociatedChunkAnswers(incomingRecommendationChunk);

        return entity;
    }

    private void RemoveOldAssociatedChunkAnswers(RecommendationChunkDbEntity incoming)
    {
        RemoveOldRelationships(incoming, (RecommendationChunkAnswerDbEntity answer, RecommendationChunkDbEntity incoming) => incoming.Answers.Exists(incomingAnswer => answer.Matches(incoming, incomingAnswer)));
    }

    private void RemoveOldAssociatedChunkContents(RecommendationChunkDbEntity incoming)
    {
        RemoveOldRelationships(incoming, (RecommendationChunkContentDbEntity content, RecommendationChunkDbEntity incoming) => incoming.Content.Exists(incomingContent => content.Matches(incoming, incomingContent)));
    }

    private async Task AddOrUpdateRecommendationChunkContents(RecommendationChunkDbEntity incoming, RecommendationChunkDbEntity existing)
    {
        static List<ContentComponentDbEntity> selectContent(RecommendationChunkDbEntity incoming) => incoming.Content;
        static bool contentMatches(ContentComponentDbEntity content, RecommendationChunkContentDbEntity chunkContent) => chunkContent.RecommendationChunkId == content.Id && chunkContent.ContentComponentId == content.Id;
        static void UpdateContentOrder(ContentComponentDbEntity content, RecommendationChunkContentDbEntity relationship)
        {
            if (relationship.ContentComponent!.Order != content.Order)
            {
                relationship.ContentComponent.Order = content.Order;
            }
        }

        await AddNewRelationshipsAndRemoveDuplicates(incoming, existing, selectContent, (Func<ContentComponentDbEntity, RecommendationChunkContentDbEntity, bool>)contentMatches, UpdateContentOrder);
    }
}