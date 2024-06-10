using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationSectionMapper(
    EntityRetriever retriever,
    EntityUpdater updater,
    CmsDbContext db,
    ILogger<RecommendationSectionMapper> logger,
    JsonSerializerOptions jsonSerialiserOptions)
    : JsonToDbMapper<RecommendationSectionDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    private readonly List<AnswerDbEntity> _incomingAnswers = [];
    private readonly List<RecommendationChunkDbEntity> _incomingContent = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        _incomingAnswers.AddRange(_entityUpdater.GetAndOrderReferencedEntities<AnswerDbEntity>(values, "answers"));
        _incomingContent.AddRange(_entityUpdater.GetAndOrderReferencedEntities<RecommendationChunkDbEntity>(values, "chunks"));

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<RecommendationSectionDbEntity>();

        if (existing != null)
        {
            existing.Answers.AddRange(await _db.RecommendationSectionAnswers
                                                .Where(recSecAnswer => recSecAnswer.RecommendationSectionId == incoming.Id)
                                                .Select(recSecAnswer => recSecAnswer.Answer)
                                                .Select(answer => answer!)
                                                .ToListAsync());

            existing.Chunks.AddRange(await _db.RecommendationSectionChunks
                                                .Where(recSecChunk => recSecChunk.RecommendationSectionId == incoming.Id)
                                                .Select(recSecChunk => recSecChunk.RecommendationChunk)
                                                .Select(chunk => chunk!)
                                                .ToListAsync());
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recSection) => recSection.Answers, _incomingAnswers, _db.Answers, false);
        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recSection) => recSection.Chunks, _incomingContent, _db.RecommendationChunks, true);
    }
}