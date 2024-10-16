namespace Dfe.PlanTech.Domain.Helpers;

public static class RandomNumber
{
    private static readonly Random _global = new();

    [ThreadStatic]
    private static Random? _local;

    public static Random Local
    {
        get
        {
            int seed;
            lock (_global)
            {
                seed = _global.Next();
            }
            _local = new Random(seed);
            return _local;
        }
    }
}
