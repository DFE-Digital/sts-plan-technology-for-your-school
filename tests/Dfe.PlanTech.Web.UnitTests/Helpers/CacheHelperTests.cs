using Dfe.PlanTech.Core.Caching.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class CacheHelperTests
{
    private readonly MemoryCache _memoryCache;
    private readonly ICacheOptions _options;
    private readonly CacheHelper _sut;

    public CacheHelperTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _options = Substitute.For<ICacheOptions>();
        _options.DefaultTimeToLive.Returns(TimeSpan.FromMinutes(5));

        _sut = new CacheHelper(_options, _memoryCache);
    }

    [Fact]
    public void Get_WhenValueExistsInCache_ReturnsCachedValue()
    {
        // Arrange
        var key = "key1";
        _memoryCache.Set(key, 42);

        // Act
        var result = _sut.Get(key, () => 99, TimeSpan.FromMinutes(1));

        // Assert
        Assert.Equal(42, result); // should come from cache, not factory
    }

    [Fact]
    public void Get_WhenValueNotInCache_CallsFactory_AndStoresValue()
    {
        // Arrange
        var key = "key2";
        int factoryCalls = 0;

        // Act
        var result = _sut.Get(key, () =>
        {
            factoryCalls++;
            return 123;
        }, TimeSpan.FromMinutes(1));

        // Assert
        Assert.Equal(123, result);
        Assert.Equal(1, factoryCalls);

        // confirm it's cached
        Assert.Equal(123, _memoryCache.Get(key));
    }

    [Fact]
    public void Get_WithoutFactory_ReturnsCachedValueOrNull()
    {
        var key = "key3";
        Assert.Equal(0, _sut.Get<int>(key));

        _memoryCache.Set(key, 55);
        Assert.Equal(55, _sut.Get<int>(key));
    }

    [Fact]
    public async Task GetAsync_WhenValueExistsInCache_ReturnsCachedValue()
    {
        var key = "key4";
        _memoryCache.Set(key, 77);

        var result = await _sut.GetAsync(key, async () =>
        {
            await Task.Delay(1);
            return 888;
        }, TimeSpan.FromMinutes(1));

        Assert.Equal(77, result); // should come from cache
    }

    [Fact]
    public async Task GetAsync_WhenValueNotInCache_CallsFactory_AndStoresValue()
    {
        var key = "key5";
        int calls = 0;

        var result = await _sut.GetAsync(key, () =>
        {
            calls++;
            return Task.FromResult(321);
        }, TimeSpan.FromMinutes(1));

        Assert.Equal(321, result);
        Assert.Equal(1, calls);
        Assert.Equal(321, _memoryCache.Get(key));
    }

    [Fact]
    public void Set_StoresValue_WithExplicitTtl()
    {
        var key = "key6";
        _sut.Set(key, TimeSpan.FromMinutes(10), 999);

        Assert.Equal(999, _memoryCache.Get(key));
    }

    [Fact]
    public void Set_StoresValue_UsingDefaultTtl()
    {
        var key = "key7";
        _sut.Set(key, 111);

        Assert.Equal(111, _memoryCache.Get(key));
    }
}
