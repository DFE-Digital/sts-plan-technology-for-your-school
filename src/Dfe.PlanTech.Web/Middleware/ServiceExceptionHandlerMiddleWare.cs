using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Exceptions;
using Dfe.PlanTech.Web.Configurations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Middleware;

public class ServiceExceptionHandlerMiddleware : IExceptionHandlerMiddleware
{
    private readonly IGetPageQuery _pageQuery;
    private readonly ErrorPagesConfiguration _errorPages;

    public ServiceExceptionHandlerMiddleware(
        IOptions<ErrorPagesConfiguration> errorPagesOptions,
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


        var internalErrorPage = await _pageQuery.GetPageById(_errorPages.InternalErrorPageId);
        var internalErrorSlug = internalErrorPage?.Slug ?? UrlConstants.Error;

        ContextRedirect(internalErrorSlug, context);
    }

    private void ContextRedirect(string internalErrorSlug, HttpContext context)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        string redirectUrl = GetRedirectUrlForException(internalErrorSlug, exception);
        context.Response.Redirect(redirectUrl);
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
