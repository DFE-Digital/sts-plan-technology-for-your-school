namespace Dfe.PlanTech.Web.Middleware;

public interface IExceptionHandlerMiddleware
{
    Task HandleExceptionAsync(HttpContext context);
}
