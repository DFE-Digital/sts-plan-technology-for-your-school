using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Establishments.Exceptions;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Domain.Users.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
namespace Dfe.PlanTech.Web.Middleware;

public class ServiceExceptionHandlerMiddleWare : IExceptionHandlerMiddleware
{
    public void ContextRedirect(HttpContext context)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        string redirectUrl = GetRedirectUrlForException(exception);

        context.Response.Redirect(redirectUrl);
    }

    static string GetRedirectUrlForException(Exception? exception) =>
        exception switch
        {
            null => UrlConstants.Error,
            ContentfulPageUnavailableException => UrlConstants.NotFound,
            ContentfulDataUnavailableException => UrlConstants.ServiceUnavailable,
            DatabaseException => UrlConstants.ServiceUnavailable,
            InvalidEstablishmentException => UrlConstants.ServiceUnavailable,
            PageNotFoundException => UrlConstants.NotFound,
            KeyNotFoundException ex when ex.Message.Contains(ClaimConstants.Organisation) => UrlConstants.OrgErrorPage,
            _ => GetRedirectUrlForException(exception.InnerException),
        };
}
