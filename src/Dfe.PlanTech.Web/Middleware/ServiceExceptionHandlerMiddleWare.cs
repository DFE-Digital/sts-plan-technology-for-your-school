using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Establishments.Exceptions;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Microsoft.AspNetCore.Diagnostics;

namespace Dfe.PlanTech.Web.Middleware;

public class ServiceExceptionHandlerMiddleWare : IExceptionHandlerMiddleware
{
    public void ContextRedirect(string internalErrorSlug, HttpContext context)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        string redirectUrl = GetRedirectUrlForException(internalErrorSlug, exception);

        context.Response.Redirect(redirectUrl);
    }

    static string GetRedirectUrlForException(string internalErrorSlug, Exception? exception) =>
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
