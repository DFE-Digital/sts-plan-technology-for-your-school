using Polly;
using Polly.Extensions.Http;

namespace Dfe.PlanTech.Web.Helpers;

public static class HttpClientPolicyExtensions
{
    public static void AddRetryPolicy(IHttpClientBuilder builder) =>
        builder
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(GetRetryPolicy());

    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
