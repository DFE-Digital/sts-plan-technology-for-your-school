using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Extensions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class RecommendationSectionMapper(EntityUpdater updater,
                                         IDatabaseHelper<ICmsDbContext> databaseHelper,
                                         ILogger<RecommendationSectionMapper> logger,
                                         JsonSerializerOptions jsonSerialiserOptions)
    : JsonToDbMapper<RecommendationSectionDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
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
            await DatabaseHelper.GetIQueryableForEntityWithoutAutoIncludes<RecommendationSectionAnswerDbEntity>().Where(recSecAnswer => recSecAnswer.RecommendationSectionId == existing.Id).Include(recSecAnswer => recSecAnswer.Answer, DatabaseHelper).ToListAsync(DatabaseHelper, default);
            await DatabaseHelper.GetIQueryableForEntityWithoutAutoIncludes<RecommendationSectionChunkDbEntity>().Where(recSecChunk => recSecChunk.RecommendationSectionId == existing.Id).Include(recSecChunk => recSecChunk.RecommendationChunk, DatabaseHelper).ToListAsync(DatabaseHelper, default);
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recSection) => recSection.Answers, _incomingAnswers, false);
        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recSection) => recSection.Chunks, _incomingContent, true);
    }
}
