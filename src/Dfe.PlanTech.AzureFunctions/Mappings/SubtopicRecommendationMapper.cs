using System.Text.Json;
using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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
    private readonly List<RecommendationIntroDbEntity> _incomingIntros = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "section", "sectionId");
        values = MoveValueToNewKey(values, "subtopic", "subtopicId");

        _incomingIntros.Clear();
        _incomingIntros.AddRange(_entityUpdater.GetAndOrderReferencedEntities<RecommendationIntroDbEntity>(values, "intros"));

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<SubtopicRecommendationDbEntity>();

        if (existing != null)
        {
            existing.RecommendationIntro = await _db.SubtopicRecommendationIntros.Where(subRecIntro => subRecIntro.SubtopicRecommendationId == incoming.Id)
                                                                        .Select(subRecIntro => subRecIntro.RecommendationIntro)
                                                                        .Select(intro => intro!)
                                                                        .ToListAsync();
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (subtopicRec) => subtopicRec.Intros, _incomingIntros, _db.RecommendationIntros, false);
    }
}