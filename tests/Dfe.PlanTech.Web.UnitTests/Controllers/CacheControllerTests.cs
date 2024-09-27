using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class CacheControllerTests
{
    private readonly IQueryCacher _queryCacher = Substitute.For<IQueryCacher>();
    private readonly ILogger<CacheController> _logger = Substitute.For<ILogger<CacheController>>();
    private readonly CacheController _cacheController;

    public CacheControllerTests()
    {
        _cacheController = new CacheController(_queryCacher, _logger);
    }

    [Fact]
    public void ClearCache_Should_Return_True_On_Success()
    {
        var clearCacheResult = _cacheController.ClearCache();

        Assert.NotNull(clearCacheResult);

        var result = clearCacheResult as ObjectResult;
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        _queryCacher.Received(1).ClearCache();
    }

    [Fact]
    public void ClearCache_Should_Return_False_On_Failure()
    {
        _queryCacher
            .When(call => call.ClearCache())
            .Do(_ => throw new Exception("unexpected error"));

        var clearCacheResult = _cacheController.ClearCache();
        Assert.NotNull(clearCacheResult);

        var result = clearCacheResult as ObjectResult;
        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
    }
}
