namespace Dfe.PlanTech.Infrastructure.Redis;

public class RedisWrapper<T>
{
    public T Value { get; set; }
    public RedisWrapper(T value) => Value = value;
}

public class RedisWrapper
{
    public static RedisWrapper<T> Create<T>(T value) => new(value);
}
