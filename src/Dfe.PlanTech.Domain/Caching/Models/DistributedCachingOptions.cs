namespace Dfe.PlanTech.Domain.Caching.Models;

public record DistributedCachingOptions(
    string ConnectionSting,
    int DistLockAcquisitionTimeoutInSeconds = 30,
    int DistLockMaxDurationInSeconds = 200)
{
    public bool HaveConnectionString => !string.IsNullOrEmpty(ConnectionSting);
}
