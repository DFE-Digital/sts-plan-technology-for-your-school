using System.Text.Json;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class SubtopicRecommendationMapper(
    EntityRetriever retriever,
    EntityUpdater updater,
    CmsDbContext db,
    ILogger<SubtopicRecommendationMapper> logger,
    JsonSerializerOptions jsonSerialiserOptions)
    : JsonToDbMapper<SubtopicRecommendationDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        var id = values["id"]?.ToString() ?? throw new KeyNotFoundException("Not found id");
        
        values = MoveValueToNewKey(values, "section", "sectionId");

        values = MoveValueToNewKey(values, "subtopic", "subtopicId");
        
        UpdateContentIds(values, id, "intros");

        return values;
    }
    
    private void UpdateContentIds(Dictionary<string, object?> values, string subtopicRecommendationId, string currentKey)
    {
        if (values.TryGetValue(currentKey, out object? intros) && intros is object[] inners)
        {
            for (var index = 0; index < inners.Length; index++)
            {
                CreateSubtopicRecommendationIntroEntity(inners[index], subtopicRecommendationId);
            }
            values.Remove(currentKey);
        }
    }
    
    
    private void CreateSubtopicRecommendationIntroEntity(object inner, string subtopicRecommendationId)
    {
        if (inner is not string recommendationIntroId)
        {
            Logger.LogWarning("Expected string but received {InnerType}", inner.GetType());
            return;
        }

        var subtopicRecommendationIntro = new SubtopicRecommendationIntroDbEntity()
        {
            SubtopicRecommendationId = subtopicRecommendationId,
            RecommendationIntroId = recommendationIntroId
        };

        _db.SubtopicRecommendationIntros.Attach(subtopicRecommendationIntro);
    }
}