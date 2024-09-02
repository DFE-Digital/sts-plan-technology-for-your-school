namespace Dfe.PlanTech.Web.Middleware;

public interface IExceptionHandlerMiddleware
{
    void ContextRedirect(HttpContext context);
}
