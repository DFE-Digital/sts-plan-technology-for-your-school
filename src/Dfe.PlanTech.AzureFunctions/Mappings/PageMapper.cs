using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dfe.PlanTech.AzureFunctions.Mappings;

public class PageMapper(PageEntityRetriever retriever, PageEntityUpdater updater, ILogger<PageMapper> logger, JsonSerializerOptions jsonSerialiserOptions) : JsonToDbMapper<PageDbEntity>(retriever, updater, logger, jsonSerialiserOptions)
{
    private const string BeforeTitleContentKey = "beforeTitleContent";
    private const string ContentKey = "content";

    private readonly List<PageContentDbEntity> _allPageContents = [];

    private readonly List<ContentComponentDbEntity> _beforeTitlePageContents = [];
    private readonly List<ContentComponentDbEntity> _pageContents = [];


    public override PageDbEntity ToEntity(CmsWebHookPayload payload)
    {
        var mappedPage = base.ToEntity(payload);

        mappedPage.AllPageContents = _allPageContents;
        mappedPage.BeforeTitleContent = _beforeTitlePageContents;
        mappedPage.Content = _pageContents;

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
        var id = values["id"]?.ToString() ?? throw new KeyNotFoundException("Not found id");

        values = MoveValueToNewKey(values, "title", "titleId");

        UpdateContentIds(values, id, BeforeTitleContentKey);
        UpdateContentIds(values, id, ContentKey);

        return values;
    }

    private void UpdateContentIds(Dictionary<string, object?> values, string pageId, string currentKey)
    {
        bool isBeforeTitleContent = currentKey == BeforeTitleContentKey;

        if (values.TryGetValue(currentKey, out object? contents) && contents is object[] inners)
        {
            for (var index = 0; index < inners.Length; index++)
            {
                CreatePageContentEntity(inners[index], index, pageId, isBeforeTitleContent);
            }

            values.Remove(currentKey);
        }
    }

    /// <summary>
    /// Creates the necessary <see cref="PageContentDbEntity"/> for the relationship 
    /// </summary>
    /// <param name="inner">The child content ID.</param>
    /// <param name="order">Order of the content for the page</param>
    /// <param name="pageId"></param>
    /// <param name="isBeforeTitleContent"></param>
    private void CreatePageContentEntity(object inner, int order, string pageId, bool isBeforeTitleContent)
    {
        if (inner is not string contentId)
        {
            Logger.LogWarning("Expected string but received {innerType}", inner.GetType());
            return;
        }

        var pageContentJoin = new PageContentDbEntity()
        {
            PageId = pageId,
            Order = order
        };

        var pageContent = new ContentComponentDbEntity() { Id = contentId };

        if (isBeforeTitleContent)
        {
            _beforeTitlePageContents.Add(pageContent);
            pageContentJoin.BeforeContentComponentId = contentId;
        }
        else
        {
            _pageContents.Add(pageContent);
            pageContentJoin.ContentComponentId = contentId;
        }

        _allPageContents.Add(pageContentJoin);
    }
}
