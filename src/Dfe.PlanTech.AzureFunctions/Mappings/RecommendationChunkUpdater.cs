using Dfe.PlanTech.AzureFunctions.Models;
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

    private void AddOrUpdateRecommendationChunkContents(RecommendationChunkDbEntity incoming, RecommendationChunkDbEntity existing)
    {
        foreach (var content in incoming.Content)
        {
            var matchingContents = Db.RecommendationChunkContents.Where(rcc => rcc.RecommendationChunkId == incoming.Id &&
                                                                                rcc.ContentComponentId == content.Id)
                                                                .OrderByDescending(pc => pc.Id)
                                                                .ToList();

            if (matchingContents.Count == 0)
            {
                existing.Content.Add(content);
                continue;
            }

            if (matchingContents.Count > 1)
            {
                Db.RecommendationChunkContents.RemoveRange(matchingContents[1..]);
            }

            var remainingMatchingContent = matchingContents[0];

            if (remainingMatchingContent.ContentComponent!.Order != content.Order)
            {
                remainingMatchingContent.ContentComponent.Order = content.Order;
            }
        }
    }
}