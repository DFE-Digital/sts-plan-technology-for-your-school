using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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

    /// <inheritdoc/>
    public async Task<IDatabase> GetDatabaseAsync(int databaseId)
    {
        var redisOptions = ConfigurationOptions.Parse(options.ConnectionString);
        redisOptions.CertificateValidation += ValidateServerCertificate;

        _connection ??= await ConnectionMultiplexer.ConnectAsync(redisOptions) ?? throw new InvalidOperationException("Failed to create Redis connection");
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
            logger.LogInformation("Attempted to flush Redis but connection was not set");
            return;
        }

        var tasks = _connection.GetEndPoints().Select(x => _connection.GetServer(x).FlushDatabaseAsync(databaseId));
        await Task.WhenAll(tasks);
    }

    public static bool ValidateServerCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

        return false;
    }

}
