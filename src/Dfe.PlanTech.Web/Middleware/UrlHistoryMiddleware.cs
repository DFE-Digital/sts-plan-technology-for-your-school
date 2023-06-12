using Dfe.PlanTech.Application.Caching.Interfaces;

namespace Dfe.PlanTech.Web.Middleware;

/// <summary>
/// Adds user page change history to cache. 
/// </summary>
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
        var pageHistory = cacher.Get(CACHE_KEY, () => new Stack<string>())!;

        var targetUrl = httpContext.Request.Host + httpContext.Request.Path;

        bool navigatingBackwards = pageHistory.TryPeek(out string? lastVisitedHistory) && !string.IsNullOrEmpty(lastVisitedHistory) && lastVisitedHistory.Contains(targetUrl);

        switch (navigatingBackwards)
        {
            case true:
                {
                    pageHistory.Pop();
                    break;
                }

            case false:
                {
                    TryAddHistory(httpContext, cacher, pageHistory, lastVisitedHistory);
                    break;
                }
        }

        await _next(httpContext);
    }

    /// <summary>
    /// Double check we're not adding duplicate history (i.e. refresh, submit, etc.) - if not, add to history.
    /// </summary>
    private static void TryAddHistory(HttpContext httpContext, ICacher cacher, Stack<string> pageHistory, string? lastVisitedHistory)
    {
        var lastUrl = httpContext.Request.Headers["Referer"].ToString();

        if (string.IsNullOrEmpty(lastUrl))
        {
            return;
        }

        bool isDuplicateUrl = pageHistory.TryPeek(out lastVisitedHistory) && !string.IsNullOrEmpty(lastVisitedHistory) && lastVisitedHistory.Equals(lastUrl);

        if (!isDuplicateUrl)
        {
            pageHistory.Push(lastUrl);
            cacher.Set(CACHE_KEY, TimeSpan.FromHours(1), pageHistory);
        }
    }
}
