using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Extensions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class RecommendationChunkMapper(EntityUpdater updater,
                                       ILogger<RecommendationChunkMapper> logger,
                                       JsonSerializerOptions jsonSerialiserOptions,
                                       IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<RecommendationChunkDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    private List<AnswerDbEntity> _incomingAnswers = [];
    private List<ContentComponentDbEntity> _incomingContent = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "csLink", "csLinkId");

        _incomingAnswers = _entityUpdater.GetAndOrderReferencedEntities<AnswerDbEntity>(values, "answers").ToList();
        _incomingContent = _entityUpdater.GetAndOrderReferencedEntities<ContentComponentDbEntity>(values, "content").ToList();

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<RecommendationChunkDbEntity>();

        if (existing != null)
        {
            //There is no need for assignment as EF Core will automatically assigned the retrieved relationships to the existing entity,
            //as the existing entity is being tracked by EF Core's ChangeTracker, and EF Core is aware of the relationship.
            if (existing.Answers == null || existing.Answers.Count == 0)
            {
                await DatabaseHelper.GetIQueryableForEntityWithoutAutoIncludes<RecommendationChunkAnswerDbEntity>().Where(recChunkAnswer => recChunkAnswer.RecommendationChunkId == existing.Id).Include(rca => rca.Answer, DatabaseHelper).ToListAsync(DatabaseHelper, CancellationToken.None);
            }

            if (existing.Content == null || existing.Content.Count == 0)
            {
                await DatabaseHelper.GetIQueryableForEntityWithoutAutoIncludes<RecommendationChunkContentDbEntity>().Where(recChunkContent => recChunkContent.RecommendationChunkId == existing.Id).Include(rca => rca.ContentComponent, DatabaseHelper).ToListAsync(DatabaseHelper, CancellationToken.None);
            }
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recChunk) => recChunk.Answers, _incomingAnswers, false);
        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recChunk) => recChunk.Content, _incomingContent, true);
    }
}
