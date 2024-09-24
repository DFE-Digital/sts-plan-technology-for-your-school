using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class SectionMapper(EntityUpdater updater,
                           ILogger<SectionMapper> logger,
                           JsonSerializerOptions jsonSerialiserOptions,
                           IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<SectionDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    private List<QuestionDbEntity> _incomingQuestions = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "interstitialPage", "interstitialPageId");

        _incomingQuestions = EntityUpdater.GetAndOrderReferencedEntities<QuestionDbEntity>(values, "questions").ToList();

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity, CancellationToken cancellationToken)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<SectionDbEntity>();

        if (existing != null)
        {
            existing.Questions = await GetEntitiesMatchingPredicate<QuestionDbEntity>(question => question.SectionId == incoming.Id, cancellationToken);
        }

        await EntityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (section) => section.Questions, _incomingQuestions, true, cancellationToken);
    }
}
