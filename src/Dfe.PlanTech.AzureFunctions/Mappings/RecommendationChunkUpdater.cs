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

        var (incoming, existing) = MapToConcreteType<RecommendationChunkDbEntity>(entity);

        AddOrUpdateRecommendationChunkAnswers(incoming, existing);
        AddOrUpdateRecommendationChunkContents(incoming, existing);
        RemoveOldAssociatedChunkContents(incoming, existing);
        RemoveOldAssociatedChunkAnswers(incoming, existing);

        return entity;
    }

    private static void RemoveOldAssociatedChunkAnswers(RecommendationChunkDbEntity incoming, RecommendationChunkDbEntity existing)
        => existing.Answers.RemoveAll(existingAnswer => !incoming.Answers.Exists(incomingAnswer => existing.Id == incomingAnswer.Id));

    private static void RemoveOldAssociatedChunkContents(RecommendationChunkDbEntity incoming, RecommendationChunkDbEntity existing)
        => existing.Content.RemoveAll(existingContent => !incoming.Content.Exists(incomingContent => existing.Id == incomingContent.Id));


    private static void AddOrUpdateRecommendationChunkAnswers(RecommendationChunkDbEntity incoming, RecommendationChunkDbEntity existing)
    {
        static List<AnswerDbEntity> selectAnswers(RecommendationChunkDbEntity incoming) => incoming.Answers;

        AddNewRelationshipsAndRemoveDuplicates<RecommendationChunkDbEntity, AnswerDbEntity, string>(incoming, existing, selectAnswers);
    }

    private static void AddOrUpdateRecommendationChunkContents(RecommendationChunkDbEntity incoming, RecommendationChunkDbEntity existing)
    {
        static List<ContentComponentDbEntity> selectContent(RecommendationChunkDbEntity incoming) => incoming.Content;

        AddNewRelationshipsAndRemoveDuplicates<RecommendationChunkDbEntity, ContentComponentDbEntity, string>(incoming, existing, selectContent);
    }
}