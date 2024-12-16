using Dfe.PlanTech.Domain.Background;
using NSubstitute;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis.UnitTests;

public class RedisCacheTestsBase
{
    protected const string Key = "testKey";
    protected const string Value = """{"this":"is json"}""";

    protected readonly IDatabase Database = Substitute.For<IDatabase>();
    protected readonly IBackgroundTaskQueue BackgroundTaskQueue = Substitute.For<IBackgroundTaskQueue>();

    protected Func<CancellationToken, Task>? QueuedFunc;
    protected string appendedString = "";

    protected readonly List<string> SetMembers = ["one", "two", "hat"];

    public RedisCacheTestsBase()
    {
        BackgroundTaskQueue.When(queue => queue.QueueBackgroundWorkItemAsync(Arg.Any<Func<CancellationToken, Task>>()))
                    .Do(task =>
                    {
                        var arg = task.ArgAt<Func<CancellationToken, Task>>(0);
                        QueuedFunc = arg;
                    });

        MockDatabaseMethods();
    }

    protected virtual void MockDatabaseMethods()
    {
        Database.StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns((callinfo) =>
        {
            var keyArg = callinfo.ArgAt<RedisKey>(0);

            if (keyArg == Key)
            {
                var redisValue = new RedisValue(Value);

                return Task.FromResult(redisValue);
            }

            return Task.FromResult(new RedisValue());
        });

        Database.StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan>(), Arg.Any<bool>(), Arg.Any<When>(), Arg.Any<CommandFlags>()).Returns((callinfo) =>
        {
            var valueArg = callinfo.ArgAt<RedisValue>(0);

            return Task.FromResult(true);
        });

        Database.KeyDeleteAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>()).Returns(true);

        Database.StringAppendAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<CommandFlags>())
                    .Returns((callinfo) =>
                    {
                        var keyArg = callinfo.ArgAt<RedisKey>(0);

                        if (keyArg != Key)
                        {
                            return -1;
                        }

                        var valueArg = callinfo.ArgAt<RedisValue>(1);

                        appendedString = $"{Value}{valueArg}";

                        return appendedString.Length;
                    });

        Database.SetMembersAsync(Key, Arg.Any<CommandFlags>()).Returns(SetMembers.Select(member => new RedisValue(member)).ToArray());
        Database.SetAddAsync(Key, Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns((callinfo) =>
        {
            var valueArg = callinfo.ArgAt<RedisValue>(1);

            SetMembers.Add(valueArg!);

            return true;
        });

        Database.SetRemoveAsync(Key, Arg.Any<RedisValue>(), Arg.Any<CommandFlags>()).Returns((callinfo) =>
        {
            var valueArg = callinfo.ArgAt<RedisValue>(1);

            return SetMembers.Remove(valueArg!);
        });
    }
}
