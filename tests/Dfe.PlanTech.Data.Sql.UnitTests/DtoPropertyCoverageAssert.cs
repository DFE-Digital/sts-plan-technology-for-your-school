using System.Reflection;

namespace Dfe.PlanTech.Data.Sql.UnitTests;

public static class DtoPropertyCoverageAssert
{
    /// <summary>
    /// Validates that all public instance properties on the DTO type have been accounted for by the test.
    /// If a DTO property is added or not asserted, this will fail with a helpful message.
    /// </summary>
    public static void AssertAllPropertiesAccountedFor<TDto>(
        IEnumerable<string> assertedPropertyNames,
        IEnumerable<string>? excludePropertyNames = null
    )
    {
        var asserted = new HashSet<string>(assertedPropertyNames);
        var excluded = excludePropertyNames is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : new HashSet<string>(excludePropertyNames, StringComparer.Ordinal);

        var allPublicProps = typeof(TDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name);

        var missing = allPublicProps
            .Where(p => !asserted.Contains(p) && !excluded.Contains(p))
            .ToArray();

        Assert.True(
            missing.Length == 0,
            $"Unasserted properties on {typeof(TDto).Name}: {string.Join(", ", missing)}"
        );
    }
}
