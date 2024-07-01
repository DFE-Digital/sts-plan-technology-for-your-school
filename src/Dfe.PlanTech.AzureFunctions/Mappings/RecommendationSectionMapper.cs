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
    private List<AnswerDbEntity> _incomingAnswers = [];
    private List<RecommendationChunkDbEntity> _incomingContent = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        _incomingAnswers = _entityUpdater.GetAndOrderReferencedEntities<AnswerDbEntity>(values, "answers").ToList();
        _incomingContent = _entityUpdater.GetAndOrderReferencedEntities<RecommendationChunkDbEntity>(values, "chunks").ToList();

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<RecommendationSectionDbEntity>();

        if (existing != null)
        {
            //There is no need for assignment as EF Core will automatically assigned the retrieved relationships to the existing entity,
            //as the existing entity is being tracked by EF Core's ChangeTracker, and EF Core is aware of the relationship.
            await _db.RecommendationSectionAnswers.IgnoreQueryFilters().Where(recSecAnswer => recSecAnswer.RecommendationSectionId == existing.Id).Include(recSecAnswer => recSecAnswer.Answer).ToListAsync();
            await _db.RecommendationSectionChunks.IgnoreQueryFilters().Where(recSecChunk => recSecChunk.RecommendationSectionId == existing.Id).Include(recSecChunk => recSecChunk.RecommendationChunk).ToListAsync();
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recSection) => recSection.Answers, _incomingAnswers, _db.Answers, false);
        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recSection) => recSection.Chunks, _incomingContent, _db.RecommendationChunks, true);
    }
}