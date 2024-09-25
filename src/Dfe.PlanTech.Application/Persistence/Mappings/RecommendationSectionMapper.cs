using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class RecommendationSectionMapper(EntityUpdater updater,
                                         ILogger<RecommendationSectionMapper> logger,
                                         JsonSerializerOptions jsonSerialiserOptions,
                                         IDatabaseHelper<ICmsDbContext> databaseHelper)
    : JsonToDbMapper<RecommendationSectionDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    private List<AnswerDbEntity> _incomingAnswers = [];
    private List<RecommendationChunkDbEntity> _incomingContent = [];

    protected override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        _incomingAnswers = EntityUpdater.GetAndOrderReferencedEntities<AnswerDbEntity>(values, "answers").ToList();
        _incomingContent = EntityUpdater.GetAndOrderReferencedEntities<RecommendationChunkDbEntity>(values, "chunks").ToList();

        return values;
    }

    protected override async Task PostUpdateEntityCallback(MappedEntity mappedEntity, CancellationToken cancellationToken)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<RecommendationSectionDbEntity>();

        if (existing != null)
        {
            //There is no need for assignment as EF Core will automatically assigned the retrieved relationships to the existing entity,
            //as the existing entity is being tracked by EF Core's ChangeTracker, and EF Core is aware of the relationship.
            await GetEntitiesMatchingPredicate<RecommendationSectionAnswerDbEntity, AnswerDbEntity>(recSecAnswer => recSecAnswer.RecommendationSectionId == existing.Id, recSecAnswer => recSecAnswer.Answer, cancellationToken);
            await GetEntitiesMatchingPredicate<RecommendationSectionChunkDbEntity, RecommendationChunkDbEntity>(recSecChunk => recSecChunk.RecommendationSectionId == existing.Id, recSecChunk => recSecChunk.RecommendationChunk, cancellationToken);
        }

        await EntityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recSection) => recSection.Answers, _incomingAnswers, false, cancellationToken);
        await EntityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recSection) => recSection.Chunks, _incomingContent, true, cancellationToken);
    }
}
