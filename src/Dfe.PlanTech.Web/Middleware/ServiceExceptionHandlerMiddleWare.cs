using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace Dfe.PlanTech.Web.Middleware;

public class ServiceExceptionHandlerMiddleware(
    IOptions<ErrorPagesConfiguration> errorPages,
    IContentfulService contentfulService
) : IExceptionHandlerMiddleware
{
    private readonly ErrorPagesConfiguration _errorPages = errorPages?.Value ?? throw new ArgumentNullException(nameof(errorPages));
    private readonly IContentfulService _contentfulService = contentfulService ?? throw new ArgumentNullException(nameof(contentfulService));

    public async Task HandleExceptionAsync(HttpContext context)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var internalErrorPage = await _contentfulService.GetPageByIdAsync(_errorPages.InternalErrorPageId);
        var internalErrorSlug = internalErrorPage?.Slug ?? UrlConstants.Error;

        ContextRedirect(internalErrorSlug, context);
    }

    private void ContextRedirect(string internalErrorSlug, HttpContext context)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        string redirectUrl = GetRedirectUrlForException(internalErrorSlug, exception);

        // Ensure the redirect URL has a leading slash but avoid double slashes
        if (!redirectUrl.StartsWith('/'))
        {
            redirectUrl = $"/{redirectUrl}";
        }

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
