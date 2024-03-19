using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationChunkUpdater(ILogger<RecommendationChunkUpdater> logger, CmsDbContext db) :EntityUpdater(logger, db)
{
    
    public override MappedEntity UpdateEntityConcrete(MappedEntity entity)
    {
        if (!entity.AlreadyExistsInDatabase)
        {
            return entity;
        }

        if (entity.ExistingEntity is not RecommendationChunkDbEntity existingRecommendationChunk)
        {
            throw new InvalidCastException($"Entity is not the expected type. {entity.ExistingEntity!.GetType()}");
        }
        
        RemoveOldAssociatedChunkContents(existingRecommendationChunk);
        RemoveOldAssociatedChunkAnswers(existingRecommendationChunk);

        return entity;
    }
    
    private void RemoveOldAssociatedChunkContents(RecommendationChunkDbEntity existingRecommendationChunk)
    {
        var contentsToRemove = Db.RecommendationChunkContents
            .Where(content => content.RecommendationChunkId == existingRecommendationChunk.Id)
            .ToList();

        Db.RecommendationChunkContents.RemoveRange(contentsToRemove);
    }
    
    private void RemoveOldAssociatedChunkAnswers(RecommendationChunkDbEntity existingRecommendationChunk)
    {
        var answersToRemove = Db.RecommendationChunkAnswers
            .Where(content => content.RecommendationChunkId == existingRecommendationChunk.Id)
            .ToList();

        Db.RecommendationChunkAnswers.RemoveRange(answersToRemove);
    }
    
}