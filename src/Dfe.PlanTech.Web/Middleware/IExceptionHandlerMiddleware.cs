namespace Dfe.PlanTech.Web.Middleware;

public interface IExceptionHandlerMiddleware
{
    void ContextRedirect(string internalErrorSlug, HttpContext context);
}
