using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class CategoryMapper(EntityUpdater updater,
                            ILogger<CategoryMapper> logger,
                            JsonSerializerOptions jsonSerialiserOptions,
                            IDatabaseHelper<ICmsDbContext> databaseHelper) : BaseJsonToDbMapper<CategoryDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    private List<SectionDbEntity> _incomingSections = [];

    protected override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "header", "headerId");

        _incomingSections = EntityUpdater.GetAndOrderReferencedEntities<SectionDbEntity>(values, "sections").ToList();

        return values;
    }

    protected override async Task PostUpdateEntityCallback(MappedEntity mappedEntity, CancellationToken cancellationToken)
    {
        var (incoming, existing) = mappedEntity.GetTypedEntities<CategoryDbEntity>();

        if (existing != null)
        {
            existing.Sections = await GetExistingSections(DatabaseHelper.Database, incoming, cancellationToken);
        }

        await EntityUpdater.UpdateReferences(incoming, existing, (category) => category.Sections, _incomingSections, true, cancellationToken);
    }

    private static async Task<List<SectionDbEntity>> GetExistingSections(ICmsDbContext db, CategoryDbEntity incoming, CancellationToken cancellationToken)
        => await db.ToListAsync(db.Sections.Where(section => section.CategoryId == incoming.Id).Select(section => section), cancellationToken);
}
