using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Exceptions;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Web.Configuration;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Middleware;

public class ServiceExceptionHandlerMiddleWare : IExceptionHandlerMiddleware
{
    private readonly IGetPageQuery _pageQuery;
    private readonly ErrorPages _errorPages;

    public ServiceExceptionHandlerMiddleWare(
        IOptions<ErrorPages> errorPagesOptions,
        IGetPageQuery getPageQuery
    )
    {
        _errorPages = errorPagesOptions.Value;
        _pageQuery = getPageQuery;
    }

    public async Task HandleExceptionAsync(HttpContext context)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        if (exception is KeyNotFoundException &&
            !exception.Message.Contains(ClaimConstants.Organisation))
        {
            await ShowNotFoundContent(context);
        }
        else
        {
            var internalErrorPage = await _pageQuery.GetPageById(_errorPages.InternalErrorPageId);
            var internalErrorSlug = internalErrorPage?.Slug ?? UrlConstants.Error;

            ContextRedirect(internalErrorSlug, context);
        }
    }

    private void ContextRedirect(string internalErrorSlug, HttpContext context)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        string redirectUrl = GetRedirectUrlForException(internalErrorSlug, exception);
        context.Response.Redirect(redirectUrl);
    }

    private async Task ShowNotFoundContent(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;

        // Create a proper ActionContext
        var actionContext = new ActionContext(
            context,
            context.GetRouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
        );

        // Create a valid UrlHelper
        var urlHelperFactory = context.RequestServices.GetService<IUrlHelperFactory>();
        var urlHelper = urlHelperFactory?.GetUrlHelper(actionContext);

        // Ensure contact link is fetched
        var contactLink = await GetContactLinkAsync(context);

        var viewModel = new NotFoundViewModel
        {
            ContactLinkHref = contactLink?.Href
        };

        var result = await RenderViewToStringAsync(context, "NotFoundError", viewModel, urlHelper);

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(result);
    }

    private async Task<string> RenderViewToStringAsync(
        HttpContext context,
        string viewName,
        object model,
        IUrlHelper urlHelper)
    {
        var viewEngine = context.RequestServices.GetService<ICompositeViewEngine>();
        var tempDataProvider = context.RequestServices.GetService<ITempDataProvider>();

        var actionContext = new ActionContext(
            context,
            context.GetRouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
        );

        using (var sw = new StringWriter())
        {
            var viewResult = viewEngine.FindView(actionContext, viewName, false);

            if (!viewResult.Success)
            {
                viewResult = viewEngine.GetView(null, $"/Views/Shared/{viewName}.cshtml", false);
            }

            if (!viewResult.Success || viewResult.View == null)
            {
                throw new InvalidOperationException($"View '{viewName}' not found.");
            }

            var view = viewResult.View;
            var viewData = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary()
            )
            {
                Model = model
            };

            var tempData = new TempDataDictionary(context, tempDataProvider);
            var viewContext = new ViewContext(
                actionContext,
                view,
                viewData,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            // Inject UrlHelper so @Url.Action works inside the view
            viewData["UrlHelper"] = urlHelper;

            await view.RenderAsync(viewContext);
            return sw.ToString();
        }
    }

    private async Task<INavigationLink?> GetContactLinkAsync(HttpContext context)
    {
        var getNavigationQuery = context.RequestServices.GetService<IGetNavigationQuery>();
        var contactOptions = context.RequestServices.GetService<IOptions<ContactOptions>>();

        if (getNavigationQuery == null || contactOptions == null)
        {
            return null;
        }

        return await getNavigationQuery.GetLinkById(contactOptions.Value.LinkId);
    }


    private static string GetRedirectUrlForException(string internalErrorSlug, Exception? exception) =>
        exception switch
        {
            null => internalErrorSlug,
            ContentfulDataUnavailableException => internalErrorSlug,
            DatabaseException => internalErrorSlug,
            InvalidEstablishmentException => internalErrorSlug,
            KeyNotFoundException ex when ex.Message.Contains(ClaimConstants.Organisation) => UrlConstants.OrgErrorPage,
            _ => GetRedirectUrlForException(internalErrorSlug, exception.InnerException),
        };
}
