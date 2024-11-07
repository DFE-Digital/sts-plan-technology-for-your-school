using System.Text.Json;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public class RedisCacheTests
{
    private readonly ILogger<RedisCache> _logger = Substitute.For<ILogger<RedisCache>>();
    private readonly IRedisConnectionManager _connectionManager = Substitute.For<IRedisConnectionManager>();
    private readonly IDatabase _database = Substitute.For<IDatabase>();
    private readonly RedisCache _cache;
    private const string key = "testKey";
    private const string value = """{"this":"is json"}""";
    private string appendedString = "";

    private readonly Answer _firstAnswer;
    private readonly Answer _secondAnswer;
    private readonly Question _question;

    private readonly List<string> _setMembers = ["one", "two", "hat"];
    public RedisCacheTests()
    {
        _cache = new RedisCache(
            connectionManager: _connectionManager,
            logger: _logger
        );

        _connectionManager.GetDatabaseAsync(Arg.Any<int>()).Returns(_database);

        _database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns((callinfo) =>
        {
            var keyArg = callinfo.ArgAt<RedisKey>(0);

            if (keyArg == key)
            {
                var redisValue = new RedisValue(value);

                return Task.FromResult(redisValue);
            }

            return Task.FromResult(new RedisValue());
        });

        _database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<bool>(), Arg.Any<When>(), Arg.Any<CommandFlags>()).Returns((callinfo) =>
        {
            var valueArg = callinfo.ArgAt<RedisValue>(0);

            return Task.FromResult(true);
        });

        _database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(true);

        _database.StringAppendAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>())
                    .Returns((callinfo) =>
                    {
                        var keyArg = callinfo.ArgAt<RedisKey>(0);

                        if (keyArg != key)
                        {
                            return -1;
                        }

                        var valueArg = callinfo.ArgAt<RedisValue>(1);

                        appendedString = $"{value}{valueArg}";

                        return appendedString.Length;
                    });

        _database.SetMembersAsync(key, Arg.Any<CommandFlags>()).Returns(_setMembers.Select(member => new RedisValue(member)).ToArray());
        _database.SetAddAsync(key, Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns((callinfo) =>
        {
            var valueArg = callinfo.ArgAt<RedisValue>(1);

            _setMembers.Add(valueArg!);

            return true;
        });

        _database.SetRemoveAsync(key, Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns((callinfo) =>
        {
            var valueArg = callinfo.ArgAt<RedisValue>(1);

            return _setMembers.Remove(valueArg!);
        });

        _firstAnswer = new Answer()
        {
            Sys = new SystemDetails { Id = "answer-one-id" },
            Maturity = "high",
            Text = "answer-one-text",
        };
        _secondAnswer = new Answer()
        {
            Sys = new SystemDetails { Id = "answer-two-id" },
            Maturity = "medium",
            Text = "answer-two-text",
        };
        _question = new Question
        {
            Sys = new SystemDetails { Id = "question-one-id" },
            Slug = "question-one-slug",
            Text = "question-one-text",
            Answers = [_firstAnswer, _secondAnswer]
        };
    }

    [Fact]
    public async Task GetOrCreateAsync_Succeeds_WithExistingKey()
    {
        var result = await _cache.GetOrCreateAsync<dynamic>(key, () => throw new Exception("This should not be called"));
        Assert.Equal(value, result);
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
    public async Task SetAsync_Succeeds()
    {
        var result = await _cache.SetAsync(key, value);
        Assert.Equal(key, result);
    }

    [Fact]
    public async Task RemoveAsync_Succeeds()
    {
        var result = await _cache.RemoveAsync(key);
        Assert.True(result);
    }

    [Fact]
    public async Task RemoveAsync_WithMultipleKeys_Succeeds()
    {
        await _cache.RemoveAsync("one", "two", "three");
        Assert.True(true);
        await _database.ReceivedWithAnyArgs(3).KeyDeleteAsync(Arg.Any<RedisKey>());
    }

    [Fact]
    public async Task RemoveAsync_WithMultipleKeys_AndDatabaseId_Succeeds()
    {
        await _cache.RemoveAsync(1, "one", "two", "three");
        Assert.True(true);
        await _database.ReceivedWithAnyArgs(3).KeyDeleteAsync(Arg.Any<RedisKey>());
    }

    [Fact]
    public async Task AppendAsync_Succeeds()
    {
        var item = "new text here";
        await _cache.AppendAsync(key, item);
        Assert.Equal(appendedString, $"{value}{item}");
    }

    [Fact]
    public async Task GetAsync_Succeeds_WithExistingKey()
    {
        var result = await _cache.GetAsync<JsonElement>(key);

        var serialised = result.Serialise();
        Assert.Equal(value, serialised);
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
        var result = await _cache.GetSetMembersAsync(key);
        Assert.Equal(_setMembers, result);
    }

    [Fact]
    public async Task SetRemoveAsync_Succeeds()
    {
        var item = _setMembers[0];
        await _cache.SetRemoveAsync(key, item);
        Assert.DoesNotContain(item, _setMembers);
    }

    [Fact]
    public async Task SetAddAsync_Succeeds()
    {
        var newItem = "newest item";
        await _cache.SetAddAsync(key, newItem);
        Assert.Contains(newItem, _setMembers);
    }

    [Fact]
    public async Task SetRemoveItemsAsync_Succeeds()
    {
        string[] items = ["one", "two", "three"];
        await _cache.SetRemoveItemsAsync(key, items);
        await _database.Received(1).SetRemoveAsync(key, Arg.Is<RedisValue[]>((value) => value.Length == items.Length && items.All(item => value.Any(redisValue => redisValue == item))));
    }

    [Fact]
    public async Task RegisterDependencyAsync_StoresAllContentIds()
    {
        await _cache.RegisterDependenciesAsync(key, _question);
        await _database.Received(1).SetAddAsync(_cache.GetDependencyKey(_question.Sys.Id), key);
        await _database.Received(1).SetAddAsync(_cache.GetDependencyKey(_firstAnswer.Sys.Id), key);
        await _database.Received(1).SetAddAsync(_cache.GetDependencyKey(_secondAnswer.Sys.Id), key);
    }

    [Fact]
    public async Task InvalidateCacheAsync_RemovesAllDependencies()
    {
        var firstAnswerKey = _cache.GetDependencyKey(_firstAnswer.Sys.Id);
        var keys = new List<string>{"one", "two", "three"};
        var dependencies = keys.Select(dep => new RedisValue(dep)).ToArray();

        _database.SetMembersAsync(firstAnswerKey, Arg.Any<CommandFlags>()).Returns(dependencies);
        await _cache.InvalidateCacheAsync(firstAnswerKey);

        await _database.Received(1).SetRemoveAsync(
            firstAnswerKey,
            Arg.Is<RedisValue[]>(arg => dependencies.All(dep => arg.Contains(dep))),
            Arg.Any<CommandFlags>());
    }
}
