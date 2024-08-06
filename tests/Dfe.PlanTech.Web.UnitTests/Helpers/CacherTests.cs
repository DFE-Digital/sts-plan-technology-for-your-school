using Dfe.PlanTech.Domain.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class CacherTests
{
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public void Get_Should_FetchFromCache_When_Exists()
    {
        var testKey = "Testing";
        var testObject = "Test value";

        _memoryCache.Set(testKey, testObject);

        var cacher = new Cacher(new CacheOptions(), _memoryCache);

        var result = cacher.Get<string>(testKey);

        Assert.NotNull(result);
        Assert.Equal(testObject, result);
    }

    [Fact]
    public void Get_Should_ReturnNull_When_NotFound()
    {
        var testKey = "Testing";
        var testObject = "Test value";

        _memoryCache.Set(testKey, testObject);

        var cacher = new Cacher(new CacheOptions(), _memoryCache);

        var result = cacher.Get<string>("doesn't exist");

        Assert.Null(result);
    }

    [Fact]
    public void Get_Should_SetValueFromService_When_NotExistingInCache()
    {
        var testKey = "Testing";
        var testObject = "Test value";

        var service = new TestService
        {
            Value = testObject
        };

        var cacher = new Cacher(new CacheOptions(), _memoryCache);

        var result = cacher.Get<string>(testKey, () => service.Value);

        Assert.NotNull(result);
        Assert.Equal(testObject, result);
        Assert.True(service.GetHasBeenCalled);

        var memoryCacheValue = _memoryCache.Get(testKey);

        Assert.NotNull(memoryCacheValue);
        Assert.Equal(testObject, memoryCacheValue);
    }

    [Fact]
    public void Set_Should_SaveInCache()
    {
        var testKey = "Testing";
        var testObject = "Test value";

        var cacher = new Cacher(new CacheOptions(), _memoryCache);

        cacher.Set(testKey, TimeSpan.FromMinutes(60), testObject);

        var cachedResult = _memoryCache.Get<string>(testKey);

        Assert.NotNull(cachedResult);
        Assert.Equal(testObject, cachedResult);
    }

    [Fact]
    public void Set_Should_Save_Using_Default_CacheOptions()
    {
        var cacheOptionsSubstitute = Substitute.For<ICacheOptions>();
        cacheOptionsSubstitute.DefaultTimeToLive
                        .Returns(TimeSpan.FromMinutes(60));

        var testKey = "Testing";
        var testObject = "Test value";

        var cacher = new Cacher(cacheOptionsSubstitute, _memoryCache);

        cacher.Set(testKey, testObject);

        var cachedResult = _memoryCache.Get<string>(testKey);

        Assert.NotNull(cachedResult);
        Assert.Equal(testObject, cachedResult);
        _ = cacheOptionsSubstitute.Received().DefaultTimeToLive;
    }

    [Fact]
    public async Task GetAsync_Should_Return_CachedValue_When_Cache_Exists()
    {
        var testKey = "Testing";
        var testObject = "Test value";

        _memoryCache.Set(testKey, testObject);

        var cacher = new Cacher(new CacheOptions(), _memoryCache);

        var result = await cacher.GetAsync(testKey, () => Task.FromResult("new value"), TimeSpan.MaxValue);

        Assert.NotNull(result);
        Assert.Equal(testObject, result);
    }

    [Fact]
    public async Task GetAsync_Should_Return_NewValue_When_Cache_NotFound()
    {
        var testKey = "Testing";
        var newValue = "new value";

        var cacher = new Cacher(new CacheOptions(), _memoryCache);

        var result = await cacher.GetAsync(testKey, () => Task.FromResult(newValue), TimeSpan.MaxValue);

        Assert.NotNull(result);
        Assert.Equal(newValue, result);
    }

    [Fact]
    public async Task GetAsync_Should_Set_NewValue_When_Cache_NotFound()
    {
        var testKey = "Testing";
        var newValue = "new value";

        var cacher = new Cacher(new CacheOptions(), _memoryCache);

        var result = await cacher.GetAsync(testKey, () => Task.FromResult(newValue), TimeSpan.MaxValue);

        var cachedResult = _memoryCache.Get(testKey);

        Assert.NotNull(cachedResult);
        Assert.Equal(newValue, cachedResult);
    }

    [Fact]
    public async Task GetAsync_Should_Set_Use_DefaultCacheValues_When_Not_Supplied()
    {
        var testKey = "Testing";
        var newValue = "new value";

        var cacheOptionsSubstitute = Substitute.For<ICacheOptions>();
        cacheOptionsSubstitute.DefaultTimeToLive.Returns(TimeSpan.FromSeconds(1));

        var cacher = new Cacher(cacheOptionsSubstitute, _memoryCache);

        var result = await cacher.GetAsync(testKey, () => Task.FromResult(newValue));

        var cachedResult = _memoryCache.Get(testKey);

        _ = cacheOptionsSubstitute.Received().DefaultTimeToLive;
    }

}

public class TestService
{
    private string? _value = null;

    public bool GetHasBeenCalled { get; private set; }
    public bool SetHasBeenCalled { get; private set; }

    public string? Value
    {
        get
        {
            GetHasBeenCalled = true;
            return _value;
        }

        set
        {
            _value = value;
            SetHasBeenCalled = true;
        }
    }
}
