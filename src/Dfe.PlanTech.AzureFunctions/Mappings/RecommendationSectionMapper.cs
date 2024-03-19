using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationSectionMapper(
    EntityRetriever retriever,
    RecommendationSectionUpdater updater,
    CmsDbContext db,
    ILogger<RecommendationSectionMapper> logger,
    JsonSerializerOptions jsonSerialiserOptions)
    : JsonToDbMapper<RecommendationSectionDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        var id = values["id"]?.ToString() ?? throw new KeyNotFoundException("Not found id");

        UpdateChunkIds(values, id, "chunks");
        UpdateAnswerIds(values, id, "answers");

        return values;
    }


    private void UpdateChunkIds(Dictionary<string, object?> values, string recommendationChunkId, string currentKey)
    {
        if (values.TryGetValue(currentKey, out object? chunks) && chunks is object[] inners)
        {
            for (var index = 0; index < inners.Length; index++)
            {
                CreateRecommendationSectionChunkEntity(inners[index], recommendationChunkId);
            }

            values.Remove(currentKey);
        }
    }

    private void CreateRecommendationSectionChunkEntity(object inner, string recommendationSectionId)
    {
        if (inner is not string chunkId)
        {
            Logger.LogWarning("Expected string but received {InnerType}", inner.GetType());
            return;
        }

        var recommendationSectionChunk = new RecommendationSectionChunkDbEntity()
        {
            RecommendationSectionId = recommendationSectionId,
            RecommendationChunkId = chunkId
        };

        _db.RecommendationSectionChunks.Attach(recommendationSectionChunk);
    }

    private void UpdateAnswerIds(Dictionary<string, object?> values, string recommendationChunkId, string currentKey)
    {
        if (values.TryGetValue(currentKey, out object? answers) && answers is object[] inners)
        {
            for (var index = 0; index < inners.Length; index++)
            {
                CreateRecommendationSectionAnswerEntity(inners[index], recommendationChunkId);
            }

            values.Remove(currentKey);
        }
    }

    private void CreateRecommendationSectionAnswerEntity(object inner, string recommendationChunkId)
    {
        if (inner is not string answerId)
        {
            Logger.LogWarning("Expected string but received {InnerType}", inner.GetType());
            return;
        }

        var recommendationSectionAnswer = new RecommendationSectionAnswerDbEntity()
        {
            RecommendationSectionId = recommendationChunkId,
            AnswerId = answerId
        };

        _db.RecommendationSectionAnswers.Attach(recommendationSectionAnswer);
    }

}