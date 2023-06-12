using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;

namespace Dfe.PlanTech.Web.Middleware;

/// <summary>
/// Adds user page change history to cache. 
/// </summary>
public class UrlHistoryMiddleware
{
    private readonly RequestDelegate _next;

    public UrlHistoryMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IUrlHistory history)
    {
        var targetUrl = httpContext.Request.Host + httpContext.Request.Path;

        var lastVisitedHistory = history.LastVisitedUrl;

        bool navigatingBackwards = !string.IsNullOrEmpty(lastVisitedHistory) && lastVisitedHistory.Contains(targetUrl);

        switch (navigatingBackwards)
        {
            case true:
                {
                    history.RemoveLastUrl();
                    break;
                }

            case false:
                {
                    TryAddHistory(httpContext, history, lastVisitedHistory);
                    break;
                }
        }

        await _next(httpContext);
    }

    /// <summary>
    /// Double check we're not adding duplicate history (i.e. refresh, submit, etc.) - if not, add to history.
    /// </summary>
    private static void TryAddHistory(HttpContext httpContext, IUrlHistory history, string? lastVisitedHistory)
    {
        var lastUrl = httpContext.Request.Headers["Referer"].ToString();

        if (string.IsNullOrEmpty(lastUrl))
        {
            return;
        }

        bool isDuplicateUrl = !string.IsNullOrEmpty(lastVisitedHistory) && lastVisitedHistory.Equals(lastUrl);

        if (!isDuplicateUrl)
        {
            history.AddUrlToHistory(lastUrl);
        }
    }
}
