using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class CategoryMapper : JsonToDbMapper<CategoryDbEntity>
{
    private readonly CmsDbContext _db;

    public CategoryMapper(CmsDbContext db, ILogger<CategoryMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
        _db = db;
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        int position = 0;
        values = MoveValueToNewKey(values, "header", "headerId");

        UpdateReferencesArray(values, "sections", _db.Sections, (id, section) =>
        {
            section.CategoryId = Payload!.Sys.Id;
            section.Order = position++;
        });

        return values;
    }
}