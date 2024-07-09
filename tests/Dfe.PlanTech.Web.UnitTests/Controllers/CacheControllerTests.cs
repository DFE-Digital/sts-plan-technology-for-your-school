using Dfe.PlanTech.Web.Controllers;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class CacheControllerTests
{
    private readonly IEFCacheServiceProvider _cacheServiceProvider = Substitute.For<IEFCacheServiceProvider>();
    private readonly ILogger<CacheController> _logger = Substitute.For<ILogger<CacheController>>();
    private readonly CacheController _cacheController;

    public CacheControllerTests()
    {
        _cacheController = new CacheController(_logger);
    }

    [Fact]
    public void ClearCache_Should_Return_True_On_Success()
    {
        Assert.True(_cacheController.ClearCache(_cacheServiceProvider));
        _cacheServiceProvider.Received(1).ClearAllCachedEntries();
    }

    [Fact]
    public void ClearCache_Should_Return_False_On_Failure()
    {
        _cacheServiceProvider
            .When(call => call.ClearAllCachedEntries())
            .Do(_ => throw new Exception("unexpected error"));
        Assert.False(_cacheController.ClearCache(_cacheServiceProvider));
    }
}
