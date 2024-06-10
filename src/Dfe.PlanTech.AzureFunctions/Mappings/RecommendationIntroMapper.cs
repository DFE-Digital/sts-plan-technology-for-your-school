using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class RecommendationIntroMapper(
    EntityRetriever retriever,
    EntityUpdater updater,
    CmsDbContext db,
    ILogger<RecommendationIntroMapper> logger,
    JsonSerializerOptions jsonSerialiserOptions)
    : JsonToDbMapper<RecommendationIntroDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    private readonly List<ContentComponentDbEntity> _incomingContent = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "header", "headerId");

        _incomingContent.AddRange(_entityUpdater.GetAndOrderReferencedEntities<ContentComponentDbEntity>(values, "content"));

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<RecommendationIntroDbEntity>();

        existing?.Content.AddRange(await _db.RecommendationIntroContents
                                                .Where(recIntroContent => recIntroContent.RecommendationIntroId == incoming.Id)
                                                .Select(recIntroContent => recIntroContent.ContentComponent)
                                                .Select(content => content!)
                                                .ToListAsync());

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recIntro) => recIntro.Content, _incomingContent, _db.ContentComponents, true);
    }
}