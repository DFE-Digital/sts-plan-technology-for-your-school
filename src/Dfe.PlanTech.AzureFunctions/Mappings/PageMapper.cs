using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class PageMapper : JsonToDbMapper<PageDbEntity>
{
    private const string BeforeTitleContentKey = "beforeTitleContent";
    private const string ContentKey = "content";
    private readonly CmsDbContext _db;

    public PageMapper(CmsDbContext db, ILogger<PageMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : base(logger, jsonSerialiserOptions)
    {
        _db = db;
    }

    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        var id = values["id"]?.ToString() ?? throw new Exception("Not found id");

        values = MoveValueToNewKey(values, "title", "titleId");

        UpdateContentIds(values, id, BeforeTitleContentKey);
        UpdateContentIds(values, id, ContentKey);

        return values;
    }

    private void UpdateContentIds(Dictionary<string, object?> values, string pageId, string currentKey)
    {
        if (values.TryGetValue(currentKey, out object? contents) && contents is object[] inners)
        {
            foreach (var inner in inners)
            {
                CreatePageContentEntity(inner, pageId, currentKey == BeforeTitleContentKey);
            }

            values.Remove(currentKey);
        }
    }

    private void CreatePageContentEntity(object inner, string pageId, bool isBeforeTitleContent)
    {
        if (inner is not string contentId)
        {
            Logger.LogWarning("Expected string but received {innerType}", inner.GetType());
            return;
        }

        var pageContent = new PageContentDbEntity()
        {
            PageId = pageId,
        };

        if (isBeforeTitleContent)
        {
            pageContent.BeforeContentComponentId = contentId;
        }
        else
        {
            pageContent.ContentComponentId = contentId;
        }

        _db.PageContents.Attach(pageContent);
    }
}
