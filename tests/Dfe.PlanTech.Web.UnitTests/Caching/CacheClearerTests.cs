using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Web.Caching;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Caching;

public class CacheClearerTests
{
    private readonly CacheClearer _cacheHandler;
    private readonly IQueryCacher _queryCacher = Substitute.For<IQueryCacher>();
    private readonly ILogger<CacheClearer> _logger = Substitute.For<ILogger<CacheClearer>>();

    public CacheClearerTests()
    {
        _cacheHandler = new CacheClearer(_queryCacher, _logger);
    }

    [Fact]
    public void CacheHandler_Should_ClearCache()
    {
        var result = _cacheHandler.ClearCache();

        Assert.True(result);

        var loggedMessages =
            _logger.GetMatchingReceivedMessages("Database cache has been cleared", LogLevel.Information);
        Assert.Single(loggedMessages);
    }

    [Fact]
    public void CacheHandler_Should_LogErrors()
    {
        var exception = new Exception("Exception thrown");
        _queryCacher.When(cp => cp.ClearCache()).Throw(exception);

        var result = _cacheHandler.ClearCache();
        Assert.False(result);

        var loggedMessages =
            _logger.GetMatchingReceivedMessages($"An error occured while trying to clear the database cache: {exception.Message}", LogLevel.Error);
        Assert.Single(loggedMessages);
    }

}
