using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class PageMapper : JsonToDbMapper<PageDbEntity>
{
    private readonly CmsDbContext _db;

    public PageMapper(CmsDbContext db, ILogger<PageMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
        _db = db;
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        var id = values["id"]?.ToString() ?? throw new Exception("Not found id");

        values = MoveValueToNewKey(values, "title", "titleId");

        UpdateContentIds(values, id, "beforeTitleContent");
        UpdateContentIds(values, id, "content");

        return values;
    }

    private void UpdateContentIds(Dictionary<string, object?> values, string pageId, string currentKey)
    {
        if (values.TryGetValue(currentKey, out object? contents) && contents is object[] inners)
        {
            foreach (var inner in inners)
            {
                UpdateBeforeTitleContentId(inner, pageId);
            }

            values.Remove(currentKey);
        }
    }

    private void UpdateBeforeTitleContentId(object inner, string pageId)
    {
        if (inner is not string id)
        {
            Logger.LogWarning("Expected string but received {innerType}", inner.GetType());
            return;
        }

        var answer = new PageContentDbEntity()
        {
            PageId = pageId,
            ContentComponentId = id
        };

        _db.PageContent.Attach(answer);
    }
}
