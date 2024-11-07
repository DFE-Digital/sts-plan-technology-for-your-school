using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

/// <summary>
/// Manages connections to Redis.
/// </summary>
public interface IRedisConnectionManager
{
    /// <summary>
    /// Creates a connection to Redis using the provided database ID
    /// </summary>
    /// <param name="databaseId"></param>
    /// <returns></returns>
    Task<IDatabase> GetDatabaseAsync(int databaseId);
}
