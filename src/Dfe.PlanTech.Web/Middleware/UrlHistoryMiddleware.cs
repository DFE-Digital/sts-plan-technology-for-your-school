using Dfe.PlanTech.Application.Caching.Interfaces;

namespace Dfe.PlanTech.Web.Middleware;

public class UrlHistoryMiddleware
{
    public const string CACHE_KEY = "UrlHistory";
    
    private readonly RequestDelegate _next;

    public UrlHistoryMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ICacher cacher)
    {
        var pageHistory = cacher.Get<Stack<string>>(CACHE_KEY, () => new Stack<string>());
        
        cacher.Set("Testing", TimeSpan.FromHours(1), "Hello");
        var existing = cacher.Get<string>("Testing");
        await _next(httpContext);
    }

}
