using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class CategoryMapper(EntityRetriever retriever, EntityUpdater updater, CmsDbContext db, ILogger<CategoryMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<CategoryDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private readonly CmsDbContext _db = db;

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        int order = 0;
        values = MoveValueToNewKey(values, "header", "headerId");

        UpdateReferencesArray(values, "sections", _db.Sections, (id, section) =>
        {
            section.CategoryId = Payload!.Sys.Id;
            section.Order = order++;
        });

        return values;
    }
}