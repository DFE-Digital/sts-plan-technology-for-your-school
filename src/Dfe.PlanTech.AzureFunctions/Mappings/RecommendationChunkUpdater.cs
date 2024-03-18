using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
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

        await AddOrUpdateRecommendationChunkContents(incoming, existing);
        RemoveOldAssociatedChunkContents(incoming);
        RemoveOldAssociatedChunkAnswers(incoming);

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
        static bool contentMatches(RecommendationChunkDbEntity incoming, ContentComponentDbEntity content, RecommendationChunkContentDbEntity chunkContent) => chunkContent.Matches(incoming, content);
        static void UpdateContentOrder(ContentComponentDbEntity content, RecommendationChunkContentDbEntity relationship)
        {
            if (relationship.ContentComponent!.Order != content.Order)
            {
                relationship.ContentComponent.Order = content.Order;
            }
        }

        await AddNewRelationshipsAndRemoveDuplicates<RecommendationChunkDbEntity, ContentComponentDbEntity, RecommendationChunkContentDbEntity>(incoming, existing, selectContent, contentMatches, UpdateContentOrder);
    }
}