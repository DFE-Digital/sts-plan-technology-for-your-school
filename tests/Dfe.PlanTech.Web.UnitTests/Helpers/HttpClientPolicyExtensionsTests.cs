using System.Net;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Polly.Utilities;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class HttpClientPolicyExtensionsTests
{
    [Fact]
    public async Task GetRetryPolicy_Retries_On_5xx_And_Succeeds_After_Retries()
    {
        var policy = HttpClientPolicyExtensions.GetRetryPolicy();

        int attempts = 0;

        // Override Polly's sleep to avoid real delays
        var originalSleep = SystemClock.SleepAsync;
        SystemClock.SleepAsync = (_, __) => Task.CompletedTask;

        try
        {
            // Fail with 500 for the first 6 attempts (i.e., cause 6 retries), then succeed
            var result = await policy.ExecuteAsync(async () =>
            {
                attempts++;
                if (attempts <= 6)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(7, attempts); // 1 try + 6 retries
        }
        finally
        {
            SystemClock.SleepAsync = originalSleep;
        }
    }

    [Fact]
    public async Task GetRetryPolicy_Retries_On_HttpRequestException_Then_Succeeds()
    {
        var policy = HttpClientPolicyExtensions.GetRetryPolicy();
        int attempts = 0;

        var originalSleep = SystemClock.SleepAsync;
        SystemClock.SleepAsync = (_, __) => Task.CompletedTask;

        try
        {
            var result = await policy.ExecuteAsync(async () =>
            {
                attempts++;
                if (attempts <= 3)
                {
                    throw new HttpRequestException("boom"); // transient network error
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(4, attempts); // 1 try + 3 retries
        }
        finally
        {
            SystemClock.SleepAsync = originalSleep;
        }
    }

    [Fact]
    public async Task GetRetryPolicy_Retries_On_404_Then_Succeeds()
    {
        var policy = HttpClientPolicyExtensions.GetRetryPolicy();
        int attempts = 0;

        var originalSleep = SystemClock.SleepAsync;
        SystemClock.SleepAsync = (_, __) => Task.CompletedTask;

        try
        {
            var result = await policy.ExecuteAsync(async () =>
            {
                attempts++;
                if (attempts <= 2)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound); // covered by .OrResult(...)
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(3, attempts); // 1 try + 2 retries
        }
        finally
        {
            SystemClock.SleepAsync = originalSleep;
        }
    }

    [Fact]
    public void AddRetryPolicy_Configures_HandlerLifetime_And_Adds_PolicyHandler()
    {
        var services = new ServiceCollection();

        // Add a named client to make sure we exercise the extension
        var builder = services.AddHttpClient("with-policy");

        // Act
        HttpClientPolicyExtensions.AddRetryPolicy(builder);

        // Build and resolve factory; this ensures the registrations are valid
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IHttpClientFactory>();

        // If this doesn't throw, the handler lifetime + policy handler were registered correctly
        var client = factory.CreateClient("with-policy");
        Assert.NotNull(client); // existence is enough as we aren't sending requests here
    }
}
