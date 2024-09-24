using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class SubtopicRecommendationMapper(EntityUpdater updater,
                                          IDatabaseHelper<ICmsDbContext> databaseHelper,
                                          ILogger<SubtopicRecommendationMapper> logger,
                                          JsonSerializerOptions jsonSerialiserOptions)
    : JsonToDbMapper<SubtopicRecommendationDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    private List<RecommendationIntroDbEntity> _incomingIntros = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "section", "sectionId");
        values = MoveValueToNewKey(values, "subtopic", "subtopicId");

        _incomingIntros = EntityUpdater.GetAndOrderReferencedEntities<RecommendationIntroDbEntity>(values, "intros").ToList();

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity, CancellationToken cancellationToken)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<SubtopicRecommendationDbEntity>();

        if (existing != null)
        {
            //There is no need for assignment as EF Core will automatically assigned the retrieved relationships to the existing entity,
            //as the existing entity is being tracked by EF Core's ChangeTracker, and EF Core is aware of the relationship.
            await GetEntitiesMatchingPredicate<SubtopicRecommendationIntroDbEntity, RecommendationIntroDbEntity>(subRecIntro => subRecIntro.SubtopicRecommendation.Id == existing.Id, subRecIntro => subRecIntro.RecommendationIntro, cancellationToken);
        }

        await EntityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (subtopicRec) => subtopicRec.Intros, _incomingIntros, false, cancellationToken);
    }
}
