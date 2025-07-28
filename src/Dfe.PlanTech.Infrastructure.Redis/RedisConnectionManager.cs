using Dfe.PlanTech.Core.Caching;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

public class RedisConnectionManager : IRedisConnectionManager
{
    private readonly ILogger<RedisConnectionManager> _logger;
    private ConnectionMultiplexer? _connection;
    private readonly DistributedCachingOptions _options;

    public RedisConnectionManager(ILoggerFactory loggerFactory, DistributedCachingOptions options)
    {
        if (string.IsNullOrEmpty(options.ConnectionString))
        {
            throw new InvalidDataException($"{nameof(options.ConnectionString)} is null or empty");
        }

        _logger = loggerFactory.CreateLogger<RedisConnectionManager>();
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<IDatabase> GetDatabaseAsync(int databaseId)
    {
        _connection ??= await ConnectionMultiplexer.ConnectAsync(_options.ConnectionString) ?? throw new InvalidOperationException("Failed to create Redis connection");
        return _connection.GetDatabase(databaseId);
    }

    /// <summary>
    /// Flushes all databases for all connections on the Redis server
    /// </summary>
    /// <param name="databaseId"></param>
    /// <returns></returns>
    public async Task FlushAsync(int databaseId = 0)
    {
        if (_connection == null)
        {
            _logger.LogInformation("Attempted to flush Redis but connection was not set");
            return;
        }

        var tasks = _connection.GetEndPoints().Select(x => _connection.GetServer(x).FlushDatabaseAsync(databaseId));
        await Task.WhenAll(tasks);
    }
}
