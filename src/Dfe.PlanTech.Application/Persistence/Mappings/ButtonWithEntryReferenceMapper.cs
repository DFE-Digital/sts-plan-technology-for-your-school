using System.Text.Json;
using Dfe.PlanTech.Application.Content;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class ButtonWithEntryReferenceMapper(EntityUpdater updater,
                                            ILogger<ButtonWithEntryReferenceMapper> logger,
                                            JsonSerializerOptions jsonSerialiserOptions,
                                            IDatabaseHelper<ICmsDbContext> databaseHelper) : BaseJsonToDbMapper<ButtonWithEntryReferenceDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
{
    protected override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        values = MoveValueToNewKey(values, "button", "buttonId");
        values = MoveValueToNewKey(values, "linkToEntry", "linkToEntryId");

        return values;
    }

    protected override Task PostUpdateEntityCallback(MappedEntity mappedEntity, CancellationToken cancellationToken)
    {
        var entity = mappedEntity.ExistingEntity ?? mappedEntity.IncomingEntity;

        DatabaseHelper.MarkNavigationAsUnchanged(entity, "Link");

        return Task.CompletedTask;
    }
}
