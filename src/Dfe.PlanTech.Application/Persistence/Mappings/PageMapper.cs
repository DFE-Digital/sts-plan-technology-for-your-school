using System.Text.Json;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Persistence.Mappings;

public class PageMapper(PageRetriever retriever,
                        PageEntityUpdater updater,
                        ILogger<PageMapper> logger,
                        JsonSerializerOptions jsonSerialiserOptions,
                        IDatabaseHelper<ICmsDbContext> databaseHelper) : JsonToDbMapper<PageDbEntity>(updater, logger, jsonSerialiserOptions, databaseHelper)
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
    protected override Dictionary<string, object?> PerformAdditionalMapping(Dictionary<string, object?> values)
    {
        if (!values.TryGetValue("id", out object? idObj) || idObj == null)
            throw new KeyNotFoundException("Not found id");

        values = MoveValueToNewKey(values, "title", "titleId");

        _pageContents.Clear();

        UpdateContentIds(values, BeforeTitleContentKey);
        UpdateContentIds(values, ContentKey);

        return values;
    }

    protected override async Task<PageDbEntity?> GetExistingEntity(PageDbEntity incomingEntity, CancellationToken cancellationToken)
    => await retriever.GetExistingDbEntity(incomingEntity, cancellationToken);

    private void UpdateContentIds(Dictionary<string, object?> values, string currentKey)
    {
        var isBeforeTitleContent = currentKey == BeforeTitleContentKey;

        if (!values.TryGetValue(currentKey, out object? contents) || contents is not object[] inners)
            return;

        for (var index = 0; index < inners.Length; index++)
        {
            CreatePageContentEntity(inners[index], index, isBeforeTitleContent);
        }

        values.Remove(currentKey);
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
