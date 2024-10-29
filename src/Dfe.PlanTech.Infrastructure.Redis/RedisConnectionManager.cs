using Dfe.PlanTech.Domain.Caching.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

public class RedisConnectionManager : IRedisConnectionManager
{
    private ConnectionMultiplexer? _connection;
    private readonly DistributedCachingOptions options;
    private readonly ILogger<RedisConnectionManager> logger;

    public RedisConnectionManager(DistributedCachingOptions options, ILogger<RedisConnectionManager> logger)
    {
        if (string.IsNullOrEmpty(options.ConnectionString))
        {
            throw new InvalidDataException($"{nameof(options.ConnectionString)} is null or empty");
        }

        this.options = options;
        this.logger = logger;
    }

    public async Task<IDatabase> GetDatabaseAsync(int databaseId)
    {
        _connection ??= await ConnectionMultiplexer.ConnectAsync(options.ConnectionString) ?? throw new InvalidOperationException("Failed to create Redis connection");
        return _connection.GetDatabase(databaseId);
    }

    public async Task FlushAsync(int databaseId = 0)
    {
        if (_connection == null)
        {
            logger.LogInformation("Attempted to flush Redis but connection was not set");
            return;
        }

        var tasks = _connection.GetEndPoints().Select(x => _connection.GetServer(x).FlushDatabaseAsync(databaseId));
        await Task.WhenAll(tasks);
    }
}
