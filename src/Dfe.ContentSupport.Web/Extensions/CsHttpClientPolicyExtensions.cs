using System.Net;
using Polly;
using Polly.Extensions.Http;

namespace Dfe.ContentSupport.Web.Extensions;

public static class CsHttpClientPolicyExtensions
{
    public static void AddRetryPolicy(IHttpClientBuilder builder)
    {
        builder
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());
    }

    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions.HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}