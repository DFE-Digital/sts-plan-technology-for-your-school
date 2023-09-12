using System.Text.Json;
using System.Text.Json.Serialization;
using Contentful.Core.Errors;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Dfe.PlanTech.Web.Helpers;

public class AuthorisationPolicy : AuthorizationHandler<PageAuthorisationRequirement>
{
  private readonly ILogger<AuthorisationPolicy> _logger;

  public AuthorisationPolicy(ILoggerFactory loggerFactory)
  {
    _logger = loggerFactory.CreateLogger<AuthorisationPolicy>();
  }

  protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PageAuthorisationRequirement requirement)
  {
    if (context.Resource is not HttpContext httpContext)
    {
      _logger.LogError("Expected resource to be HttpContext but received {type}", context.Resource?.GetType());
      return;
    }

    bool success = await ProcessPage(httpContext);

    if (success)
    {
      context.Succeed(requirement);
    }
    else
    {
      context.Fail();
    }
  }

  private async Task<bool> ProcessPage(HttpContext httpContext)
  {
    var endpoint = httpContext.GetEndpoint();

    if (endpoint == null)
    {
      _logger.LogError("Endpoint is null but should have value");
      return false;
    }

    string slug = GetRequestRoute(httpContext);

    var scope = httpContext.RequestServices.CreateAsyncScope();
    using var pageQuery = scope.ServiceProvider.GetRequiredService<GetPageQuery>();

    Page page = await GetPageForSlug(httpContext, slug, pageQuery);

    var asJson = JsonSerializer.Serialize(page);
    httpContext.Items.Add("page", page);

    return !page.RequiresAuthorisation || UserIsAuthorised(httpContext, page);
  }

  private static async Task<Page> GetPageForSlug(HttpContext httpContext, string slug, GetPageQuery pageQuery)
  => await pageQuery.GetPageBySlug(slug, httpContext.RequestAborted) ?? throw new KeyNotFoundException($"Could not find page with slug {slug}");

  private static bool UserIsAuthorised(HttpContext httpContext, Page page)
  => page.RequiresAuthorisation && httpContext.User.Identity?.IsAuthenticated == true;

  private static string GetRequestRoute(HttpContext httpContext)
  {
    var slug = httpContext.Request.RouteValues["route"];

    if (slug == null || slug is not string slugString)
    {
      slugString = "/";
    }

    httpContext.Request.RouteValues["route"] = slugString;

    return slugString;
  }
}


public class PageAuthorisationRequirement : IAuthorizationRequirement
{
  public PageAuthorisationRequirement()
  {

  }
}

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

    var pageItem = bindingContext.HttpContext.Items["page"];

    if (pageItem == null)
    {
      return Task.CompletedTask;
    }

    var page = pageItem as Page;

    if (page == null)
    {
      _logger.LogWarning("Page is null");
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
