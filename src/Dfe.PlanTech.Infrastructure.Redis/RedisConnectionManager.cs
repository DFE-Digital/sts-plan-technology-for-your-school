using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

public class RedisConnectionManager(DistributedCachingOptions options, ILogger<RedisConnectionManager> logger)
{
    private ConnectionMultiplexer? _connection;

    public async Task<IDatabase> GetDatabaseAsync(int databaseId) {
        _connection ??= await ConnectionMultiplexer.ConnectAsync(options.ConnectionSting) ?? throw new InvalidOperationException("Failed to create Redis connection");
        return _connection.GetDatabase(databaseId);
    }

    public async Task FlushAsync(int databaseId = 0)
    {
        if(_connection == null){
            logger.LogInformation("Attempted to flush Redis but connection was not set");
            return;
        }

        var tasks =  _connection.GetEndPoints().Select(x => _connection.GetServer(x).FlushDatabaseAsync(databaseId));
        await Task.WhenAll(tasks);
    }
}
