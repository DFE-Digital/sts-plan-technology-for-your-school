using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Web.DbWriter.Mappings;

public class RecommendationIntroMapper(
    EntityRetriever retriever,
    EntityUpdater updater,
    CmsDbContext db,
    ILogger<RecommendationIntroMapper> logger,
    JsonSerializerOptions jsonSerialiserOptions)
    : JsonToDbMapper<RecommendationIntroDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    private List<ContentComponentDbEntity> _incomingContent = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "header", "headerId");

        _incomingContent = _entityUpdater.GetAndOrderReferencedEntities<ContentComponentDbEntity>(values, "content").ToList();

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<RecommendationIntroDbEntity>();

        if (existing != null && existing.Content != null && existing.Content.Count == 0)
        {
            //There is no need for assignment as EF Core will automatically assigned the retrieved relationships to the existing entity,
            //as the existing entity is being tracked by EF Core's ChangeTracker, and EF Core is aware of the relationship.
            await _db.RecommendationIntroContents.IgnoreQueryFilters().Where(recChunkIntro => recChunkIntro.RecommendationIntroId == existing.Id).Include(recIntro => recIntro.ContentComponent).ToListAsync();
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (recIntro) => recIntro.Content, _incomingContent, _db.ContentComponents, true);
    }
}
