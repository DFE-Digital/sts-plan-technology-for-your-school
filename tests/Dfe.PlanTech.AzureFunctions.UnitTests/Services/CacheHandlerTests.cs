using Dfe.PlanTech.AzureFunctions.Services;
using Dfe.PlanTech.AzureFunctions.UnitTests.Helpers;
using Dfe.PlanTech.Domain.Persistence.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Services;

public class CacheHandlerTests
{
    private const string _refresh_api_key_name = "X-WEBSITE-CACHE-CLEAR-API-KEY";
    private const string _refresh_api_key_value = "mock-refresh-api-key";
    private const string _refresh_endpoint = "http://mock-refresh-endpoint/";

    private readonly CacheRefreshConfiguration _cacheRefreshConfiguration;
    private readonly HttpClient _httpClient;
    private readonly MockHttpHandler _httpMessageHandler;
    private readonly CacheHandler _cacheHandler;
    private readonly ILogger<CacheHandler> _logger;

    public CacheHandlerTests()
    {
        _cacheRefreshConfiguration = new CacheRefreshConfiguration(_refresh_endpoint, _refresh_api_key_name, _refresh_api_key_value);
        _httpMessageHandler = new MockHttpHandler();
        _httpClient = new HttpClient(_httpMessageHandler);
        _logger = Substitute.For<ILogger<CacheHandler>>();
        _cacheHandler = new CacheHandler(_httpClient, _cacheRefreshConfiguration, _logger);
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
