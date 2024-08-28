namespace Dfe.PlanTech.Web.Middleware;

public interface IExceptionHandlerMiddleware
{
    Task HandleException(HttpContext context);
}
