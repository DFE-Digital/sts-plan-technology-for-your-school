using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationChunkMapper(RecommendationChunkRetriever retriever, RecommendationChunkUpdater updater, CmsDbContext db, ILogger<RecommendationChunkMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<RecommendationChunkDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        var id = values["id"]?.ToString() ?? throw new KeyNotFoundException("Not found id");

        values = MoveValueToNewKey(values, "header", "headerId");

        UpdateContentIds(values, id, "content");
        UpdateAnswerIds(values, id, "answers");

        return values;
    }

    private void UpdateContentIds(Dictionary<string, object?> values, string recommendationChunkId, string currentKey)
    {
        if (values.TryGetValue(currentKey, out object? contents) && contents is object[] inners)
        {
            for (var index = 0; index < inners.Length; index++)
            {
                CreateRecommendationContentEntity(inners[index], recommendationChunkId);
            }
            values.Remove(currentKey);
        }
    }

    private void UpdateAnswerIds(Dictionary<string, object?> values, string recommendationChunkId, string currentKey)
    {
        if (values.TryGetValue(currentKey, out object? contents) && contents is object[] inners)
        {
            for (var index = 0; index < inners.Length; index++)
            {
                CreateRecommendationAnswerEntity(inners[index], recommendationChunkId);
            }
            values.Remove(currentKey);
        }
    }

    private void CreateRecommendationAnswerEntity(object inner, string recommendationChunkId)
    {
        if (inner is not string answerId)
        {
            Logger.LogWarning("Expected string but received {InnerType}", inner.GetType());
            return;
        }

        var recommendationChunkAnswer = new RecommendationChunkAnswerDbEntity()
        {
            RecommendationChunkId = recommendationChunkId,
            AnswerId = answerId
        };

        _db.RecommendationChunkAnswers.Attach(recommendationChunkAnswer);
    }

    private void CreateRecommendationContentEntity(object inner, string recommendationChunkId)
    {
        if (inner is not string contentId)
        {
            Logger.LogWarning("Expected string but received {InnerType}", inner.GetType());
            return;
        }

        var recommendationChunkContent = new RecommendationChunkContentDbEntity()
        {
            RecommendationChunkId = recommendationChunkId,
            ContentComponentId = contentId
        };

        _db.RecommendationChunkContents.Attach(recommendationChunkContent);
    }

}