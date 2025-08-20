using Dfe.PlanTech.Core.Caching;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.UnitTests.Shared.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;
public class RedisLockProviderUnitTests
{
    private readonly DistributedCachingOptions _options = new("", 1, 1);
    private readonly IRedisConnectionManager _connectionManager = Substitute.For<IRedisConnectionManager>();
    private readonly ILogger<RedisLockProvider> _logger = Substitute.For<ILogger<RedisLockProvider>>();
    private readonly IDatabase _database = Substitute.For<IDatabase>();

    private readonly RedisLockProvider _provider;

    public RedisLockProviderUnitTests()
    {
        _provider = new(_logger, _connectionManager, _options);
        _connectionManager.GetDatabaseAsync(Arg.Any<int>()).Returns(_database);
    }

    [Fact]
    public async Task LockReleaseAsync_Succeeds()
    {
        const string key = "key";
        const string value = "lockvalue";

        await _provider.LockReleaseAsync(key, value);
        await _database.Received(1).LockReleaseAsync(key, value, CommandFlags.DemandMaster);
    }

    [Fact]
    public async Task LockExtendAsync_Succeeds()
    {
        const string key = "key";
        const string value = "lockvalue";

        var timespan = TimeSpan.FromSeconds(100);
        await _provider.LockExtendAsync(key, value, timespan);
        await _database.Received(1).LockExtendAsync(key, value, timespan, CommandFlags.DemandMaster);
    }

    [Fact]
    public async Task WaitForLockAsync_Succeeds()
    {
        const string key = "key";

        _database.LockTakeAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<CommandFlags>()).Returns(true);

        var result = await _provider.WaitForLockAsync(key, true);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task WaitForLockAsync_DoesNot_ThrowException_OnFailure()
    {
        const string key = "key";

        _database.LockTakeAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<CommandFlags>()).Returns(false);

        var result = await _provider.WaitForLockAsync(key, false);

        Assert.Null(result);
    }

    [Fact]
    public async Task WaitForLockAsync_ThrowsException_OnFailure()
    {
        const string key = "key";

        _database.LockTakeAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<CommandFlags>()).Returns(false);

        await Assert.ThrowsAnyAsync<LockException>(() => _provider.WaitForLockAsync(key, true));
    }

    [Fact]
    public async Task LockAndRun_Succeeds()
    {
        const string key = "key";

        var runnable = Substitute.For<IMockRunnable>();

        _database.LockTakeAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<CommandFlags>()).Returns(true);

        await _provider.LockAndRun(key, runnable.Action);

        await runnable.Received(1).Action();
        await _database.Received(1).LockReleaseAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), CommandFlags.DemandMaster);
    }

    [Fact]
    public async Task LockAndRun_Errors_WhenLockFailure()
    {
        const string key = "key";

        var runnable = Substitute.For<IMockRunnable>();

        _database.LockTakeAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<CommandFlags>()).Returns(false);

        await Assert.ThrowsAnyAsync<Exception>(() => _provider.LockAndRun(key, runnable.Action));
        await runnable.Received(0).Action();
        await _database.Received(0).LockReleaseAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), CommandFlags.DemandMaster);
    }

    [Fact]
    public async Task LockAndRun_LogsErroredActions()
    {
        const string key = "key";
        const string thrownErrorMessage = "Error occurred";

        _database.LockTakeAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<CommandFlags>()).Returns(true);

        await _provider.LockAndRun(key, () => throw new Exception(thrownErrorMessage));

        var loggedMessage = _logger.ReceivedLogMessages().FirstOrDefault(message => message.LogLevel == LogLevel.Error && message.Message.Contains("Error while executing locked operation for key:"));
        Assert.NotNull(loggedMessage);

        await _database.Received(1).LockReleaseAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), CommandFlags.DemandMaster);
    }

    [Fact]
    public async Task LockAndGet_Succeeds()
    {
        const string key = "key";
        const string returnValue = "returnValue";

        var runnable = Substitute.For<IMockRunnable>();
        runnable.Func().Returns(returnValue);

        _database.LockTakeAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<CommandFlags>()).Returns(true);

        var result = await _provider.LockAndGet(key, runnable.Func);

        await runnable.Received(1).Func();
        Assert.Equal(returnValue, result);
    }

    [Fact]
    public async Task LockAndGet_Errors_WhenLockFailure()
    {
        const string key = "key";
        const string returnValue = "returnValue";

        var runnable = Substitute.For<IMockRunnable>();
        runnable.Func().Returns(returnValue);

        _database.LockTakeAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<CommandFlags>()).Returns(false);

        await Assert.ThrowsAnyAsync<Exception>(() => _provider.LockAndGet(key, runnable.Func));
        await runnable.Received(0).Func();
    }

    [Fact]
    public async Task LockAndGet_LogsErroredActions()
    {
        const string key = "key";
        const string thrownErrorMessage = "Error occurred";
        static Func<Task<string>> func() => throw new Exception(thrownErrorMessage);

        _database.LockTakeAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<CommandFlags>()).Returns(true);

        await _provider.LockAndGet(key, () => Task.FromResult(func()));

        var loggedMessage = _logger.ReceivedLogMessages().FirstOrDefault(message => message.LogLevel == LogLevel.Error && message.Message.Contains("Error while executing locked operation for key:"));
        Assert.NotNull(loggedMessage);

        await _database.Received(1).LockReleaseAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), CommandFlags.DemandMaster);
    }
}

public interface IMockRunnable
{
    Task Action();

    Task<string> Func();
}
