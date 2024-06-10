using Dfe.PlanTech.AzureFunctions.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class CategoryMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<CategoryMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<CategoryDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;
    private readonly List<SectionDbEntity> _incomingSections = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "header", "headerId");

        _incomingSections.AddRange(_entityUpdater.GetAndOrderReferencedEntities<SectionDbEntity>(values, "sections"));

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<CategoryDbEntity>();

        if (existing != null)
        {
            existing.Sections = await _db.Sections
                                            .Where(section => section.CategoryId == incoming.Id)
                                            .Select(section => section)
                                            .ToListAsync();
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (category) => category.Sections, _incomingSections, _db.Sections, true);
    }
}