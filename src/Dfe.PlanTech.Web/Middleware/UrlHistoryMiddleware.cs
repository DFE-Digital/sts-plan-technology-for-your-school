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

        var targetUrl = httpContext.Request.Host + httpContext.Request.Path;

        bool navigatingBackwards = pageHistory.TryPeek(out string? lastVisitedHistory) && !string.IsNullOrEmpty(lastVisitedHistory) && lastVisitedHistory.Contains(targetUrl);

        if (navigatingBackwards)
        {
            pageHistory.Pop();
        }

        if (!navigatingBackwards)
        {
            var lastUrl = httpContext.Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(lastUrl))
            {
                bool isDuplicateUrl = pageHistory.TryPeek(out lastVisitedHistory) && !string.IsNullOrEmpty(lastVisitedHistory) && lastVisitedHistory.Equals(lastUrl);

                if (!isDuplicateUrl)
                {
                    pageHistory.Push(lastUrl);
                    cacher.Set(CACHE_KEY, TimeSpan.FromHours(1), pageHistory);
                }
            }
        }

        await _next(httpContext);
    }

}
