namespace Dfe.PlanTech.Cache.Redis;

internal static class RandomNumber
{
    private static Random _global = new Random();
    [ThreadStatic]
    private static Random _local;

    public static int Next()
    {
        Random inst = _local;
        if (inst == null)
        {
            int seed;
            lock (_global)
                seed = _global.Next();
            _local = inst = new Random(seed);
        }
        return inst.Next();
    }

    /// <summary>
    /// Returns a random number
    /// </summary>
    /// <param name="min">The inclusive MIN value</param>
    /// <param name="max">The EXCLUSIVE max value</param>
    /// <returns></returns>
    public static int Next(int min, int max)
    {
        Random inst = _local;
        if (inst == null)
        {
            int seed;
            lock (_global)
                seed = _global.Next();
            _local = inst = new Random(seed);
        }
        return inst.Next(min, max);
    }
}
