using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationIntroMapper(
    EntityRetriever retriever,
    RecommendationIntroUpdater updater,
    CmsDbContext db,
    ILogger<RecommendationIntroMapper> logger,
    JsonSerializerOptions jsonSerialiserOptions)
    : JsonToDbMapper<RecommendationIntroDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        var id = values["id"]?.ToString() ?? throw new KeyNotFoundException("Not found id");

        values = MoveValueToNewKey(values, "header", "headerId");

        UpdateContentIds(values, id, "content");

        return values;
    }

    private void UpdateContentIds(Dictionary<string, object?> values, string recommendationIntroId, string currentKey)
    {
        if (values.TryGetValue(currentKey, out object? contents) && contents is object[] inners)
        {
            for (var index = 0; index < inners.Length; index++)
            {
                CreateRecommendationContentEntity(inners[index], recommendationIntroId);
            }
            values.Remove(currentKey);
        }
    }

    private void CreateRecommendationContentEntity(object inner, string recommendationIntroId)
    {
        if (inner is not string contentId)
        {
            Logger.LogWarning("Expected string but received {InnerType}", inner.GetType());
            return;
        }

        var recommendationIntroContent = new RecommendationIntroContentDbEntity()
        {
            RecommendationIntroId = recommendationIntroId,
            ContentComponentId = contentId
        };

        _db.RecommendationIntroContents.Attach(recommendationIntroContent);
    }
}