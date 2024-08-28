using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Domain.Establishments.Exceptions;
using Dfe.PlanTech.Domain.SignIns.Enums;
using Dfe.PlanTech.Domain.Users.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
namespace Dfe.PlanTech.Web.Middleware;

public class ServiceExceptionHandlerMiddleWare : IExceptionHandlerMiddleware
{
    public readonly UserJourneyMissingContentExceptionHandler UserJourneyExceptionHandler;

    public ServiceExceptionHandlerMiddleWare(UserJourneyMissingContentExceptionHandler userJourneyExceptionHandler)
    {
        UserJourneyExceptionHandler = userJourneyExceptionHandler;
    }

    public async Task HandleException(HttpContext context)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        await UserJourneyExceptionHandler.TryHandleException(exception, context.RequestAborted);

        ContextRedirect(context, exception);
    }

    public void ContextRedirect(HttpContext context, Exception? exception)
    {
        string redirectUrl = GetRedirectUrlForException(exception);

        context.Response.Redirect(redirectUrl);
    }

    static string GetRedirectUrlForException(Exception? exception) =>
        exception switch
        {
            null => UrlConstants.Error,
            UserAccessUnavailableException => UrlConstants.CombinedErrorPage,
            UserAccessRoleNotFoundException => UrlConstants.CombinedErrorPage,
            ContentfulDataUnavailableException => UrlConstants.ServiceUnavailable,
            DatabaseException => UrlConstants.ServiceUnavailable,
            InvalidEstablishmentException => UrlConstants.ServiceUnavailable,
            PageNotFoundException => UrlConstants.ServiceUnavailable,
            KeyNotFoundException ex when ex.Message.Contains(ClaimConstants.Organisation) => UrlConstants.CombinedErrorPage,
            _ => GetRedirectUrlForException(exception.InnerException),
        };
}
