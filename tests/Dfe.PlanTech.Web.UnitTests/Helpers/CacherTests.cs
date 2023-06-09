using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Moq;
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