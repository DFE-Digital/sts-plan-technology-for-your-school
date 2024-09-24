using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class CategoryMapper(EntityUpdater updater,
                            ICmsDbContext db,
                            ILogger<CategoryMapper> logger,
                            JsonSerializerOptions jsonSerialiserOptions,
                            IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<CategoryDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    private List<SectionDbEntity> _incomingSections = [];

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "header", "headerId");

        _incomingSections = _entityUpdater.GetAndOrderReferencedEntities<SectionDbEntity>(values, "sections").ToList();

        return values;
    }

    public override async Task PostUpdateEntityCallback(MappedEntity mappedEntity, CancellationToken cancellationToken)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<CategoryDbEntity>();

        if (existing != null)
        {
            existing.Sections = await GetExistingSections(db, incoming, cancellationToken);
        }

        await _entityUpdater.UpdateReferences(incomingEntity: incoming, existingEntity: existing, (category) => category.Sections, _incomingSections, true, cancellationToken);
    }

    private static async Task<List<SectionDbEntity>> GetExistingSections(ICmsDbContext db, CategoryDbEntity incoming, CancellationToken cancellationToken)
        => await db.ToListAsync(db.Sections.Where(section => section.CategoryId == incoming.Id).Select(section => section), cancellationToken);
}
