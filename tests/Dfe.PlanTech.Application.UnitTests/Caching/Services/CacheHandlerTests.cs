using Dfe.PlanTech.Application.Caching.Services;
using Dfe.PlanTech.Application.UnitTests.TestHelpers;
using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Caching.Services;

public class CacheHandlerTests
{
    private const string _refresh_api_key_name = "X-WEBSITE-CACHE-CLEAR-API-KEY";
    private const string _refresh_api_key_value = "mock-refresh-api-key";
    private const string _refresh_endpoint = "http://mock-refresh-endpoint/";

    private readonly MockHttpHandler _httpMessageHandler = new();
    private readonly CacheHandler _cacheHandler;

    public CacheHandlerTests()
    {
        var cacheRefreshConfiguration = new CacheRefreshConfiguration(_refresh_endpoint, _refresh_api_key_name, _refresh_api_key_value);
        var httpClient = new HttpClient(_httpMessageHandler);
        var logger = Substitute.For<ILogger<CacheHandler>>();
        _cacheHandler = new CacheHandler(httpClient, cacheRefreshConfiguration, logger);
    }

    [Fact]
    public async Task CacheHandler_Should_Make_Request_With_CorrectHeaders()
    {
        await _cacheHandler.RequestCacheClear(default);
        Assert.Single(_httpMessageHandler.Requests);
        var request = _httpMessageHandler.Requests.First();
        Assert.NotNull(request.RequestUri);
        Assert.Equal(_refresh_endpoint, request.RequestUri.ToString());
        Assert.True(request.Headers.TryGetValues(_refresh_api_key_name, out var headerValues));
        Assert.Contains(_refresh_api_key_value, headerValues);
    }
}
