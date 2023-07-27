using System.Text.Json;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Web.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Helpers;

public class CacherTests
{
    private readonly IDistributedCache _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    [Fact]
    public async Task Get_Should_FetchFromCache_When_Exists()
    {
        var testKey = "Testing";
        var testObject = "Test value";

        await _cache.SetStringAsync(testKey, ConvertAndSerialise(testObject));

        ICacher cacher = new Cacher(_cache);

        var result = await cacher.GetAsync<string>(testKey);

        Assert.NotNull(result);
        Assert.Equal(testObject, result);
    }

    [Fact]
    public async Task Get_Should_ReturnNull_When_NotFound()
    {
        var testKey = "Testing";
        var testObject = "Test value";

        await _cache.SetStringAsync(testKey, ConvertAndSerialise(testObject));

        ICacher cacher = new Cacher(_cache);

        var result = await cacher.GetAsync<string>("doesn't exist");

        Assert.Null(result);
    }

    [Fact]
    public async Task Get_Should_SetValueFromService_When_NotExistingInCache()
    {
        var testKey = "Testing";
        var testObject = "Test value";

        var service = new TestService
        {
            Value = testObject
        };

        ICacher cacher = new Cacher(_cache);

        var result = await cacher.GetAsync(testKey, () => service.Value);

        Assert.NotNull(result);
        Assert.Equal(testObject, result);
        Assert.True(service.GetHasBeenCalled);

        var memoryCacheValue = await _cache.GetStringAsync(testKey);
        Assert.NotNull(memoryCacheValue);

        var asCachedItem = Deserialise<string>(memoryCacheValue!);
        Assert.NotNull(asCachedItem);
        Assert.Equal(testObject, asCachedItem.Item);
    }

    [Fact]
    public async Task Set_Should_SaveInCache()
    {
        var testKey = "Testing";
        var testObject = "Test value";

        ICacher cacher = new Cacher(_cache);

        await cacher.SetAsync(testKey, testObject);

        var cachedResult = await _cache.GetStringAsync(testKey);

        Assert.NotNull(cachedResult);

        var asCachedItem = Deserialise<string>(cachedResult!);
        Assert.Equal(testObject, asCachedItem.Item);
    }

    [Fact]
    public async Task GetAsync_Should_Return_CachedValue_When_Cache_Exists()
    {
        var testKey = "Testing";
        var testObject = "Test value";

        await _cache.SetStringAsync(testKey, ConvertAndSerialise(testObject));

        ICacher cacher = new Cacher(_cache);

        var result = await cacher.GetAsync(testKey, () => Task.FromResult("new value"));

        Assert.NotNull(result);
        Assert.Equal(testObject, result);
    }

    [Fact]
    public async Task GetAsync_Should_Return_NewValue_When_Cache_NotFound()
    {
        var testKey = "Testing";
        var newValue = "new value";

        ICacher cacher = new Cacher(_cache);

        var result = await cacher.GetAsync(testKey, () => Task.FromResult(newValue), TimeSpan.FromHours(1));

        Assert.NotNull(result);
        Assert.Equal(newValue, result);
    }

    [Fact]
    public async Task GetAsync_Should_Set_NewValue_When_Cache_NotFound()
    {
        var testKey = "Testing";
        var newValue = "new value";

        ICacher cacher = new Cacher(_cache);

        var result = await cacher.GetAsync(testKey, () => Task.FromResult(newValue), TimeSpan.FromHours(1));

        var cachedResult = await _cache.GetStringAsync(testKey);
        Assert.NotNull(cachedResult);

        var asCachedItem = Deserialise<string>(cachedResult);
        
        Assert.Equal(newValue, asCachedItem.Item);
    }

    [Fact]
    public async Task GetAsync_Should_SerialiseObject()
    {
        var testKey = "Testing";
        TestClass newValue = new("Test string", 124, null);

        ICacher cacher = new Cacher(_cache);

        var result = await cacher.GetAsync(testKey, () => Task.FromResult(newValue));

        Assert.Equivalent(newValue, result);
    }

    private CachedItem<T?> ConvertToCachedItem<T>(T? value) => new(value);
    private string Serialise<T>(T? value) => JsonSerializer.Serialize(value);
    private string ConvertAndSerialise<T>(T? value) => Serialise(ConvertToCachedItem(value));
    private CachedItem<T?> Deserialise<T>(string value) => JsonSerializer.Deserialize<CachedItem<T?>>(value) ??
                                                           throw new Exception("Invalid JSON");
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

public record TestClass(string? StringValue, int? IntValue, bool? BoolValue);
