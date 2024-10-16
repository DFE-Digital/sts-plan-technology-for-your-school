namespace Dfe.PlanTech.Infrastructure.Redis;

public abstract class ConnectionString
{
    protected readonly string _connectionString;
    public ConnectionString(string dataConnectionString) => _connectionString = dataConnectionString ?? throw new ArgumentNullException(nameof(dataConnectionString));
    public override string ToString() => _connectionString;
}

public class RedisConnectionString : ConnectionString
{
    public RedisConnectionString(string dataConnectionString) : base(dataConnectionString) { }
    public static implicit operator string(RedisConnectionString d) => d._connectionString;
    public static implicit operator RedisConnectionString(string d) => new(d);
}
