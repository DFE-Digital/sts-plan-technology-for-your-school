using StackExchange.Redis;

namespace Dfe.PlanTech.Infrastructure.Redis;

public interface IRedisConnectionManager
{
    Task<IDatabase> GetDatabaseAsync(int databaseId);
}
