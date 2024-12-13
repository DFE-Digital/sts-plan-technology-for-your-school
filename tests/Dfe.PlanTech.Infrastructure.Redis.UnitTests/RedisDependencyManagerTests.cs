using System.Reflection;
using NSubstitute;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public class RedisDependencyManagerTests : RedisCacheTestsBase
{
    private readonly RedisDependencyManager _dependencyManager;

    public RedisDependencyManagerTests()
    {
        _dependencyManager = new(BackgroundTaskQueue);
    }

    [Fact]
    public async Task RegisterDependencyAsync_StoresAllContentIds()
    {
        var batch = Substitute.For<IBatch>();
        Database.CreateBatch().Returns(batch);

        await _dependencyManager.RegisterDependenciesAsync(Database, Key, RedisCacheTestHelpers.Question);

        Assert.NotNull(QueuedFunc);

        await QueuedFunc(default);

        await batch.Received(1).SetAddAsync(_dependencyManager.GetDependencyKey(RedisCacheTestHelpers.Question.Sys.Id), Key);
        await batch.Received(1).SetAddAsync(_dependencyManager.GetDependencyKey(RedisCacheTestHelpers.FirstAnswer.Sys.Id), Key);
        await batch.Received(1).SetAddAsync(_dependencyManager.GetDependencyKey(RedisCacheTestHelpers.SecondAnswer.Sys.Id), Key);
    }

    [Fact]
    public void GetDependencies_ThrowsException_When_Value_NotIContentComponent()
    {
        var testValue = "this should throw an exception";

        var batch = Substitute.For<IBatch>();
        Database.CreateBatch().Returns(batch);

        var method = _dependencyManager.GetType().GetMethod("GetDependencies", BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        Assert.ThrowsAny<InvalidOperationException>(() => method.Invoke(_dependencyManager, [testValue]));
    }
}
