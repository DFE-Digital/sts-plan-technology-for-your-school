using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationChunkMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<RecommendationChunkMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<RecommendationChunkDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;

    private readonly List<AnswerDbEntity> _incomingAnswers = [];
    private readonly List<ContentComponentDbEntity> _incomingContent = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "header", "headerId");

        _incomingAnswers.Clear();
        _incomingContent.Clear();

        _incomingAnswers.AddRange(_entityUpdater.GetAndOrderReferencedEntities<AnswerDbEntity>(values, "answers"));
        _incomingContent.AddRange(_entityUpdater.GetAndOrderReferencedEntities<ContentComponentDbEntity>(values, "content"));

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<RecommendationChunkDbEntity>();

        if (existing != null)
        {
            existing.Answers.AddRange(await _db.RecommendationChunkAnswers
                                                .Where(recChunkAnswer => recChunkAnswer.AnswerId == incoming.Id)
                                                .Select(recChunkAnswer => recChunkAnswer.Answer)
                                                .Select(content => content!)
                                                .ToListAsync());

            existing.Content.AddRange(await _db.RecommendationChunkContents
                                                .Where(recChunkContent => recChunkContent.RecommendationChunkId == incoming.Id)
                                                .Select(recChunkContent => recChunkContent.ContentComponent)
                                                .Select(content => content!)
                                                .ToListAsync());
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recChunk) => recChunk.Answers, _incomingAnswers, _db.Answers, true);
        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recChunk) => recChunk.Content, _incomingContent, _db.ContentComponents, true);
    }
}