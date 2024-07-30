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
    private List<RecommendationIntroDbEntity> _incomingIntros = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "section", "sectionId");
        values = MoveValueToNewKey(values, "subtopic", "subtopicId");

        _incomingIntros = _entityUpdater.GetAndOrderReferencedEntities<RecommendationIntroDbEntity>(values, "intros").ToList();

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<SubtopicRecommendationDbEntity>();

        if (existing != null)
        {
            //There is no need for assignment as EF Core will automatically assigned the retrieved relationships to the existing entity,
            //as the existing entity is being tracked by EF Core's ChangeTracker, and EF Core is aware of the relationship.
            await _db.SubtopicRecommendationIntros.IgnoreQueryFilters().Where(subRecIntro => subRecIntro.SubtopicRecommendation.Id == existing.Id).Include(subRecIntro => subRecIntro.RecommendationIntro).ToListAsync();
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (subtopicRec) => subtopicRec.Intros, _incomingIntros, _db.RecommendationIntros, false);
    }
}
