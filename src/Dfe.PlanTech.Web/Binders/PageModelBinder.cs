using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.PlanTech.Web.Binders;

/// <summary>
/// Retrieves <see cref="Page"/> from the HttpContext items, as set by <see cref="Authorisation.Policies.PageModelAuthorisationPolicy"/>
/// </summary>
public class PageModelBinder(
    ILoggerFactory loggerFactory
) : IModelBinder
{
    private readonly ILogger<PageModelBinder> _logger = loggerFactory.CreateLogger<PageModelBinder>();

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext, nameof(bindingContext));

        if (!bindingContext.HttpContext.Items.TryGetValue(nameof(CmsPageDto), out var pageItem))
        {
            _logger.LogError("Page is not set");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        if (pageItem is not CmsPageDto page)
        {
            _logger.LogError("Page is not {type}", typeof(CmsPageDto));
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(page);
        return Task.CompletedTask;
    }
}
