using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Caching;

[ExcludeFromCodeCoverage]
public record DistributedCachingOptions(
    string ConnectionString,
    int DistLockAcquisitionTimeoutInSeconds = 30,
    int DistLockMaxDurationInSeconds = 200)
{
    public bool HaveConnectionString => !string.IsNullOrEmpty(ConnectionString);
}
