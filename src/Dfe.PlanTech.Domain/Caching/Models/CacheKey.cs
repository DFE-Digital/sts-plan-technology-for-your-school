namespace Dfe.PlanTech.Domain.Caching.Models;

internal static class CacheKey
{
    public static string Make(string name, string subname, params object[] items) => $"{name}.{subname}({string.Join(',', items.Select(x => x?.ToString()))})";
}
