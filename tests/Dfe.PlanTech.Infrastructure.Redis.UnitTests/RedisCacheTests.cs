using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public class RedisCacheTests : RedisCacheTestsBase
{
    private readonly ILogger<RedisCache> _logger = Substitute.For<ILogger<RedisCache>>();
    private readonly IRedisConnectionManager _connectionManager = Substitute.For<IRedisConnectionManager>();
    private readonly IRedisDependencyManager _dependencyManager = Substitute.For<IRedisDependencyManager>();
    private readonly RedisCache _cache;

    public RedisCacheTests()
    {
        _cache = new RedisCache(
            connectionManager: _connectionManager,
            logger: _logger,
            dependencyManager: _dependencyManager,
            backgroundTaskQueue: BackgroundTaskQueue
        );

        _connectionManager.GetDatabaseAsync(Arg.Any<int>()).Returns(Database);
    }

    [Fact]
    public async Task GetOrCreateAsync_Succeeds_WithExistingKey()
    {
        var result = await _cache.GetOrCreateAsync<dynamic>(Key, () => throw new Exception("This should not be called"));
        Assert.Equal(Value, result);
    }

    [Fact]
    public async Task GetOrCreateAsync_Succeeds_WithNewKey()
    {
        string otherValue = """{ "thisis" : "another value" }""";
        var serialised = JsonSerialiser.Deserialise<JsonElement>(otherValue);
        Task<JsonElement> action() => Task.FromResult(serialised);

        var result = await _cache.GetOrCreateAsync("some other key", action);
        Assert.Equal(serialised, result);
    }

    [Fact]
    public async Task GetOrCreateAsync_Calls_OnItemCreationCallBack_OnSuccess()
    {
        int timesModified = 0;
        string otherValue = """{ "thisis" : "another value" }""";
        var serialised = JsonSerialiser.Deserialise<JsonElement>(otherValue);
        Task<JsonElement> action() => Task.FromResult(serialised);

        var result = await _cache.GetOrCreateAsync("some other key", action, onCacheItemCreation: (json) => Task.FromResult(timesModified++));
        Assert.Equal(serialised, result);
    }

    [Fact]
    public async Task GetOrCreateAsync_LogsWarning_OnNullActionResult()
    {
        string otherValue = """{ "thisis" : "another value" }""";
        var serialised = JsonSerialiser.Deserialise<JsonElement>(otherValue);
        JsonElement? nullResult = null;
        Task<JsonElement?> action() => Task.FromResult(nullResult);

        var result = await _cache.GetOrCreateAsync("some other key", action);
        Assert.Null(result);
        var loggedMessages = _logger.ReceivedLogMessages();
        var matching = loggedMessages.FirstOrDefault(msg => msg.LogLevel == LogLevel.Warning && msg.Message.Contains("Action returned null for cache item with key"));
        Assert.NotNull(matching);
    }

    [Fact]
    public async Task GetOrCreateAsync_Handles_GetErrors()
    {
        string otherValue = """{ "thisis" : "another value" }""";
        var serialised = JsonSerialiser.Deserialise<JsonElement>(otherValue);
        Task<JsonElement> action() => Task.FromResult(serialised);

        Database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).ThrowsAsync((callinfo) =>
                {
                    throw new Exception("Error");
                });

        var result = await _cache.GetOrCreateAsync(Key, action);
        Assert.Equal(serialised, result);
    }

    [Fact]
    public async Task SetAsync_Succeeds()
    {
        var result = await _cache.SetAsync(Key, Value);
        Assert.Equal(Key, result);
    }

    [Fact]
    public async Task RemoveAsync_Succeeds()
    {
        var result = await _cache.RemoveAsync(Key);
        Assert.True(result);
    }

    [Fact]
    public async Task RemoveAsync_WithMultipleKeys_Succeeds()
    {
        await _cache.RemoveAsync("one", "two", "three");
        Assert.True(true);
        await Database.ReceivedWithAnyArgs(3).KeyDeleteAsync(Arg.Any<RedisKey>());
    }

    [Fact]
    public async Task RemoveAsync_WithMultipleKeys_AndDatabaseId_Succeeds()
    {
        await _cache.RemoveAsync(1, "one", "two", "three");
        Assert.True(true);
        await Database.ReceivedWithAnyArgs(3).KeyDeleteAsync(Arg.Any<RedisKey>());
    }

    [Fact]
    public async Task AppendAsync_Succeeds()
    {
        var item = "new text here";
        await _cache.AppendAsync(Key, item);
        Assert.Equal(appendedString, $"{Value}{item}");
    }

    [Fact]
    public async Task GetAsync_Succeeds_WithExistingKey()
    {
        var result = await _cache.GetAsync<JsonElement>(Key);

        var serialised = result.Serialise();
        Assert.Equal(Value, serialised);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetAsync_Handles_NullOrEmptyKey(string? key)
    {
        var defaultString = default(string);
        var result = await _cache.GetAsync<string>(key!);

        Assert.Equal(defaultString, result);

        var matchingLoggedMessage = _logger.ReceivedLogMessages().FirstOrDefault(msg => msg.LogLevel == LogLevel.Error && msg.Message.Equals("Attempt made to retrieve items with empty key"));
        Assert.NotNull(matchingLoggedMessage);
    }

    [Fact]
    public async Task GetSetMembersAsync_Succeeds()
    {
        var result = await _cache.GetSetMembersAsync(Key);
        Assert.Equal(SetMembers, result);
    }

    [Fact]
    public async Task SetRemoveAsync_Succeeds()
    {
        var item = SetMembers[0];
        await _cache.SetRemoveAsync(Key, item);
        Assert.DoesNotContain(item, SetMembers);
    }

    [Fact]
    public async Task SetAddAsync_Succeeds()
    {
        var newItem = "newest item";
        await _cache.SetAddAsync(Key, newItem);
        Assert.Contains(newItem, SetMembers);
    }

    [Fact]
    public async Task SetRemoveItemsAsync_Succeeds()
    {
        string[] items = ["one", "two", "three"];
        await _cache.SetRemoveItemsAsync(Key, items);
        await Database.Received(1).SetRemoveAsync(Key, Arg.Is<RedisValue[]>((value) => value.Length == items.Length && items.All(item => value.Any(redisValue => redisValue == item))));
    }

    [Fact]
    public async Task InvalidateCacheAsync_RemovesAllDependencies()
    {
        var firstAnswerKey = _dependencyManager.GetDependencyKey(RedisCacheTestHelpers.FirstAnswer.Sys.Id);
        var keys = new List<string> { "one", "two", "three" };
        var dependencies = keys.Select(dep => new RedisValue(dep)).ToArray();

        Database.SetMembersAsync(firstAnswerKey, Arg.Any<CommandFlags>()).Returns(dependencies);
        await _cache.InvalidateCacheAsync(RedisCacheTestHelpers.FirstAnswer.Sys.Id, "Answer");

        Assert.NotNull(QueuedFunc);

        await QueuedFunc(default);
        await Database.Received(1).SetRemoveAsync(
            firstAnswerKey,
            Arg.Is<RedisValue[]>(arg => dependencies.All(dep => arg.Contains(dep))),
            Arg.Any<CommandFlags>());
    }
}
