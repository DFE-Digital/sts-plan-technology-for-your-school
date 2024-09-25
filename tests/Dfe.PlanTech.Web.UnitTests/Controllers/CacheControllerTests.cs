using Dfe.PlanTech.Application.Extensions;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class CacheControllerTests
{
    private readonly ILogger<CacheController> _logger = Substitute.For<ILogger<CacheController>>();
    private readonly CacheController _cacheController;

    private readonly IQueryable<ContentComponent> _mockQueryable = Substitute.For<IQueryable<ContentComponent>>();
    private int _queryCallCount;

    public CacheControllerTests()
    {
        _cacheController = new CacheController(_logger);

        _mockQueryable
            .When(query => query.ToListAsync())
            .Do(_ => _queryCallCount++);
    }

    [Fact]
    public void ClearCache_Should_Empty_Cache()
    {
        _mockQueryable.ToListAsyncWithCache();
        _mockQueryable.ToListAsyncWithCache();

        Assert.Equal(1, _queryCallCount);

        var clearCacheResult = _cacheController.ClearCache();
        Assert.NotNull(clearCacheResult);

        var result = clearCacheResult as ObjectResult;
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        _mockQueryable.ToListAsyncWithCache();

        Assert.Equal(2, _queryCallCount);
    }
}
