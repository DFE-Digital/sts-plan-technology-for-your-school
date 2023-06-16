using Dfe.PlanTech.Application.Caching.Interfaces;

namespace Dfe.PlanTech.Web.Middleware;

/// <summary>
/// Adds user page change history to cache. 
/// </summary>
public class UrlHistoryMiddleware
{
    private readonly ILogger<UrlHistoryMiddleware> _logger;
    private readonly RequestDelegate _next;

    public UrlHistoryMiddleware(ILogger<UrlHistoryMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IUrlHistory history)
    {
        ProcessRequestUri(httpContext, history);

        await _next(httpContext);
    }

    private void ProcessRequestUri(HttpContext httpContext, IUrlHistory history)
    {
        try
        {
            Uri targetUrl = GetRequestUri(httpContext.Request);

            var lastUrl = history.LastVisitedUrl;

            bool navigatingBackwards = UrlsMatch(lastUrl, targetUrl);

            _logger.LogTrace("Navigating to {targetUrl} from {lastUrl}. Navigating backwards is {navigatingBackwards}",
                            lastUrl,
                            targetUrl,
                            navigatingBackwards);

            switch (navigatingBackwards)
            {
                case true:
                    {
                        history.RemoveLastUrl();
                        break;
                    }

                case false:
                    {
                        TryAddHistory(httpContext, history, lastUrl);
                        break;
                    }
            }
        }
        catch (UriFormatException ex)
        {
            _logger.LogError("Error processing {host} and {path} - {message}", httpContext.Request.Host, httpContext.Request.Path, ex.Message);
        }
    }

    /// <summary>
    /// Double check we're not adding duplicate history (i.e. refresh, submit, etc.) - if not, add to history.
    /// </summary>
    private void TryAddHistory(HttpContext httpContext, IUrlHistory history, Uri? lastVisitedHistory)
    {
        if (!TryGetRefererUri(httpContext, out Uri? refererUri) || refererUri == null)
        {
            return;
        }

        bool isDuplicateUrl = UrlsMatch(lastVisitedHistory, refererUri);

        _logger.LogTrace("navigating to {refererUri}. last visited was {lastVisitedHistory}. is duplicate is {isDuplicateUrl}", refererUri, lastVisitedHistory, isDuplicateUrl);

        if (!isDuplicateUrl)
        {
            history.AddUrlToHistory(refererUri);
        }
    }

    /// <summary>
    /// Checks to ensure the PathAndQuery of both URIs are equal
    /// </summary>
    /// <param name="lastUrl"></param>
    /// <param name="otherUrl"></param>
    /// <returns></returns>
    private static bool UrlsMatch(Uri? lastUrl, Uri otherUrl) => lastUrl != null && lastUrl.LocalPath.Equals(otherUrl.LocalPath);

    private static Uri GetRequestUri(HttpRequest request)
    {
        var fullPath = string.Format("{0}://{1}{2}{3}", request.Scheme, request.Host, request.Path, request.QueryString);

        return new Uri(fullPath);
    }

    private static bool TryGetRefererUri(HttpContext httpContext, out Uri? refererUri)
    {
        var lastUrl = httpContext.Request.Headers["Referer"].ToString();

        if (string.IsNullOrEmpty(lastUrl))
        {
            refererUri = null;
            return false;
        }

        refererUri = new Uri(lastUrl);
        return true;
    }
}
