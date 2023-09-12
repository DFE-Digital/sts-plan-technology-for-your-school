using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Dfe.PlanTech.Web.Binders;

/// <summary>
/// Retrieves <see cref="Page"/> from the HttpContext items, as set by <see cref="Authorisation.PageModelAuthorisationPolicy"/>  
/// </summary>
public class PageModelBinder : IModelBinder
{
  private readonly ILogger<PageModelBinder> _logger;

  public PageModelBinder(ILogger<PageModelBinder> logger)
  {
    _logger = logger;
  }

  public Task BindModelAsync(ModelBindingContext bindingContext)
  {
    if (bindingContext == null)
    {
      throw new ArgumentNullException(nameof(bindingContext));
    }

    var pageItem = bindingContext.HttpContext.Items[nameof(Page)];

    if (pageItem == null)
    {
      _logger.LogWarning("Page is not set");
      bindingContext.Result = ModelBindingResult.Failed();
      return Task.CompletedTask;
    }

    if (pageItem is not Page page)
    {
      _logger.LogWarning("Page is not {type}", typeof(Page));
      bindingContext.Result = ModelBindingResult.Failed();
      return Task.CompletedTask;
    }

    bindingContext.Result = ModelBindingResult.Success(page);
    return Task.CompletedTask;
  }
}

public class PageModelBinderProvider : IModelBinderProvider
{
  public IModelBinder? GetBinder(ModelBinderProviderContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    if (context.Metadata.ModelType == typeof(Page))
    {
      return new BinderTypeModelBinder(typeof(PageModelBinder));
    }

    return null;
  }
}