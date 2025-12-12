using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.PlanTech.Web.Binders;

/// <summary>
/// Retrieves <see cref="Page"/> from the HttpContext items, as set by <see cref="Authorisation.Policies.PageModelAuthorisationPolicy"/>
/// </summary>
public class PageModelBinder(
    ILogger<PageModelBinder> logger
) : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        if (!bindingContext.HttpContext.Items.TryGetValue(nameof(PageEntry), out var pageItem))
        {
            logger.LogError("Page is not set");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        if (pageItem is not PageEntry page)
        {
            logger.LogError("Page is not of type {Type}", typeof(PageEntry));
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(page);
        return Task.CompletedTask;
    }
}
