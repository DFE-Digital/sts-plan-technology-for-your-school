using System.Text.Json.Serialization;
using Dfe.PlanTech.Application.Caching.Interfaces;

namespace Dfe.PlanTech.Web.Middleware;

/// <summary>
/// Adds user page change history to cache. 
/// </summary>
public class UrlHistoryMiddleware
{
    public const string REFERER_HEADER_KEY = "Referer";

    private readonly ILogger<UrlHistoryMiddleware> _logger;
    private readonly RequestDelegate _next;

    public UrlHistoryMiddleware(ILogger<UrlHistoryMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IUrlHistory history)
    {
        await ProcessRequestUri(httpContext, history);

        await _next(httpContext);
    }

    private async Task ProcessRequestUri(HttpContext httpContext, IUrlHistory history)
    {
        //If not logged in, don't bother
        if (httpContext.User == null || !httpContext.User.Claims.Any()) return;

        if (!TryGetRequestUri(httpContext.Request, out Uri? targetUrl) || targetUrl == null) return;

        var lastUrl = await history.GetLastVisitedUrl();

        bool navigatingBackwards = UrlsMatch(lastUrl, targetUrl);

        _logger.LogTrace("Navigating to {targetUrl} from {lastUrl}. Navigating backwards is {navigatingBackwards}",
                        lastUrl,
                        targetUrl,
                        navigatingBackwards);

        switch (navigatingBackwards)
        {
            case true:
                {
                    await history.RemoveLastUrl();
                    break;
                }

            case false:
                {
                    await TryAddHistory(httpContext, history, lastUrl);
                    break;
                }
        }
    }

    /// <summary>
    /// Double check we're not adding duplicate history (i.e. refresh, submit, etc.) - if not, add to history.
    /// </summary>
    private async Task TryAddHistory(HttpContext httpContext, IUrlHistory history, Uri? lastVisitedHistory)
    {
        if (!TryGetRefererUri(httpContext, out Uri? refererUri) || refererUri == null)
        {
            return;
        }

        bool isDuplicateUrl = UrlsMatch(lastVisitedHistory, refererUri);

        _logger.LogTrace("navigating to {refererUri}. last visited was {lastVisitedHistory}. is duplicate is {isDuplicateUrl}", refererUri, lastVisitedHistory, isDuplicateUrl);

        if (!isDuplicateUrl)
        {
            await history.AddUrlToHistory(refererUri);
        }
    }

    /// <summary>
    /// Checks to ensure the PathAndQuery of both URIs are equal
    /// </summary>
    /// <param name="lastUrl"></param>
    /// <param name="otherUrl"></param>
    /// <returns></returns>
    private static bool UrlsMatch(Uri? lastUrl, Uri otherUrl) => lastUrl != null && lastUrl.LocalPath.Equals(otherUrl.LocalPath);

    /// <summary>
    /// Creates a Uri from the request fields
    /// </summary>
    /// <param name="lastUrl"></param>
    /// <param name="otherUrl"></param>
    /// <returns></returns>
    private bool TryGetRequestUri(HttpRequest request, out Uri? requestUri)
    {
        try
        {
            var fullPath = string.Format("{0}://{1}{2}{3}", request.Scheme, request.Host, request.Path, request.QueryString);
            requestUri = new Uri(fullPath);
            return true;
        }
        catch (UriFormatException ex)
        {
            _logger.LogError("Error processing {host} and {path} - {message}", request.Host, request.Path, ex.Message);
            requestUri = null;
            return false;
        }
    }

    private static bool TryGetRefererUri(HttpContext httpContext, out Uri? refererUri)
    {
        if (!httpContext.Request.Headers.ContainsKey(REFERER_HEADER_KEY))
        {
            refererUri = null;
            return false;
        }

        var lastUrl = httpContext.Request.Headers[REFERER_HEADER_KEY].ToString();

        if (string.IsNullOrEmpty(lastUrl))
        {
            refererUri = null;
            return false;
        }

        refererUri = new Uri(lastUrl);
        return true;
    }
}
