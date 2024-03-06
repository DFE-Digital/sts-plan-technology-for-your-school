using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationChunkMapper : JsonToDbMapper<RecommendationChunkDbEntity>
{
    private readonly CmsDbContext _db;
    
    public RecommendationChunkMapper(CmsDbContext db, ILogger<JsonToDbMapper<RecommendationChunkDbEntity>> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
        _db = db;
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        var id = values["id"]?.ToString() ?? throw new KeyNotFoundException("Not found id");

        values = MoveValueToNewKey(values, "header", "headerId");
        
        UpdateContentIds(values, id, "content");
        UpdateContentIds(values, id, "answers");

        return values;
    }
    
    
    private void UpdateContentIds(Dictionary<string, object?> values, string recommendationChunkId, string currentKey)
    {
        if (values.TryGetValue(currentKey, out object? contents) && contents is object[] inners)
        {
            for (var index = 0; index < inners.Length; index++)
            {
                CreateRecommendationContentEntity(inners[index], index, recommendationChunkId);
            }

            values.Remove(currentKey);
        }
    }
    
    
    private void UpdateAnswerIds(Dictionary<string, object?> values, string recommendationChunkId, string currentKey)
    {
        if (values.TryGetValue(currentKey, out object? contents) && contents is object[] inners)
        {
            values.Remove(currentKey);
        }
    }
    
    private void CreateRecommendationContentEntity(object inner, int order, string recommendationChunkId)
    {
        if (inner is not string contentId)
        {
            Logger.LogWarning("Expected string but received {innerType}", inner.GetType());
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