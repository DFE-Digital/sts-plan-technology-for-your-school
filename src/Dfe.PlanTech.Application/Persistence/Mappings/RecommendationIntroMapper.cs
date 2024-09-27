using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class RecommendationIntroMapper(EntityUpdater updater,
                                       ILogger<RecommendationIntroMapper> logger,
                                       JsonSerializerOptions jsonSerialiserOptions,
                                       IDatabaseHelper<ICmsDbContext> databaseHelper)
    : BaseJsonToDbMapper<RecommendationIntroDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    private List<ContentComponentDbEntity> _incomingContent = [];

    protected override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "header", "headerId");

        _incomingContent = EntityUpdater.GetAndOrderReferencedEntities<ContentComponentDbEntity>(values, "content").ToList();

        return values;
    }

    protected override async Task PostUpdateEntityCallback(MappedEntity mappedEntity, CancellationToken cancellationToken)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<RecommendationIntroDbEntity>();

        if (existing != null && existing.Content != null && existing.Content.Count == 0)
        {
            //There is no need for assignment as EF Core will automatically assigned the retrieved relationships to the existing entity,
            //as the existing entity is being tracked by EF Core's ChangeTracker, and EF Core is aware of the relationship.
            await GetEntitiesMatchingPredicate<RecommendationIntroContentDbEntity, ContentComponentDbEntity>(recChunkIntro => recChunkIntro.RecommendationIntroId == existing.Id, recIntro => recIntro.ContentComponent, cancellationToken);
        }

        await EntityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recIntro) => recIntro.Content, _incomingContent, true, cancellationToken);
    }
}
