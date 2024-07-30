using System.Text.Json;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class PageMapper(PageEntityRetriever retriever, PageEntityUpdater updater, ILogger<PageMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<PageDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private const string BeforeTitleContentKey = "beforeTitleContent";
    private const string ContentKey = "content";

    private readonly List<PageContentDbEntity> _pageContents = [];

    public List<PageContentDbEntity> PageContents => _pageContents;

    public override PageDbEntity ToEntity(CmsWebHookPayload payload)
    {
        var mappedPage = base.ToEntity(payload);
        foreach (var content in PageContents)
        {
            mappedPage.AllPageContents.Add(content);
        }

        return mappedPage;
    }

    /// <summary>
    /// Create joins for content, and before title content, and map the title ID to the correct expected name.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        if (!values.TryGetValue("id", out object? idObj) || idObj == null)
            throw new KeyNotFoundException("Not found id");

        values = MoveValueToNewKey(values, "title", "titleId");

        _pageContents.Clear();

        UpdateContentIds(values, BeforeTitleContentKey);
        UpdateContentIds(values, ContentKey);

        return values;
    }

    private void UpdateContentIds(Dictionary<string, object?> values, string currentKey)
    {
        bool isBeforeTitleContent = currentKey == BeforeTitleContentKey;

        if (values.TryGetValue(currentKey, out object? contents) && contents is object[] inners)
        {
            for (var index = 0; index < inners.Length; index++)
            {
                CreatePageContentEntity(inners[index], index, isBeforeTitleContent);
            }

            values.Remove(currentKey);
        }
    }

    /// <summary>
    /// Creates the necessary <see cref="PageContentDbEntity"/> for the relationship 
    /// </summary>
    /// <param name="inner">The child content ID.</param>
    /// <param name="order">Order of the content for the page</param>
    /// <param name="isBeforeTitleContent"></param>
    private void CreatePageContentEntity(object inner, int order, bool isBeforeTitleContent)
    {
        if (inner is not string contentId)
        {
            Logger.LogWarning("Expected string but received {innerType}", inner.GetType());
            return;
        }

        var pageContent = new PageContentDbEntity()
        {
            Order = order,
            BeforeContentComponentId = isBeforeTitleContent ? contentId : null,
            ContentComponentId = !isBeforeTitleContent ? contentId : null,
        };

        _pageContents.Add(pageContent);
    }
}
