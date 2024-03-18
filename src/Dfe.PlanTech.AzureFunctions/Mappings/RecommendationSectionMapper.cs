using System.Text.Json;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationSectionMapper(
    RecommendationSectionRetriever retriever,
    RecommendationSectionUpdater updater,
    ILogger<RecommendationSectionMapper> logger,
    JsonSerializerOptions jsonSerialiserOptions)
    : JsonToDbMapper<RecommendationSectionDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly List<RecommendationChunkDbEntity> _sectionChunks = [];
    private readonly List<AnswerDbEntity> _sectionAnswers = [];

    public override RecommendationSectionDbEntity ToEntity(CmsWebHookPayload payload)
    {
        var recommendationSection = base.ToEntity(payload);

        recommendationSection.Answers.AddRange(_sectionAnswers);
        recommendationSection.Chunks.AddRange(_sectionChunks);

        return recommendationSection;
    }

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

        var recommendationSectionChunk = new RecommendationChunkDbEntity()
        {
            Id = chunkId
        };

        _sectionChunks.Add(recommendationSectionChunk);
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

        var recommendationSectionAnswer = new AnswerDbEntity()
        {
            Id = answerId
        };

        _sectionAnswers.Add(recommendationSectionAnswer);
    }
}