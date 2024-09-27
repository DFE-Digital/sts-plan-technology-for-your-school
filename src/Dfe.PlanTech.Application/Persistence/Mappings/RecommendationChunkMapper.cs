using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class RecommendationChunkMapper(EntityUpdater updater,
                                       ILogger<RecommendationChunkMapper> logger,
                                       JsonSerializerOptions jsonSerialiserOptions,
                                       IDatabaseHelper<ICmsDbContext> databaseHelper) : BaseJsonToDbMapper<RecommendationChunkDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    private List<AnswerDbEntity> _incomingAnswers = [];
    private List<ContentComponentDbEntity> _incomingContent = [];

    protected override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "csLink", "csLinkId");

        _incomingAnswers = EntityUpdater.GetAndOrderReferencedEntities<AnswerDbEntity>(values, "answers").ToList();
        _incomingContent = EntityUpdater.GetAndOrderReferencedEntities<ContentComponentDbEntity>(values, "content").ToList();

        return values;
    }

    protected override async Task PostUpdateEntityCallback(MappedEntity mappedEntity, CancellationToken cancellationToken)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<RecommendationChunkDbEntity>();

        if (existing != null)
        {
            //There is no need for assignment as EF Core will automatically assigned the retrieved relationships to the existing entity,
            //as the existing entity is being tracked by EF Core's ChangeTracker, and EF Core is aware of the relationship.
            if (existing.Answers == null || existing.Answers.Count == 0)
            {
                await GetEntitiesMatchingPredicate<RecommendationChunkAnswerDbEntity, AnswerDbEntity>(recChunkAnswer => recChunkAnswer.RecommendationChunkId == existing.Id, rca => rca.Answer, cancellationToken);
            }

            if (existing.Content == null || existing.Content.Count == 0)
            {
                await GetEntitiesMatchingPredicate<RecommendationChunkContentDbEntity, ContentComponentDbEntity>(recChunkContent => recChunkContent.RecommendationChunkId == existing.Id, rca => rca.ContentComponent, cancellationToken);
            }
        }

        await EntityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recChunk) => recChunk.Answers, _incomingAnswers, false, cancellationToken);
        await EntityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recChunk) => recChunk.Content, _incomingContent, true, cancellationToken);
    }
}
