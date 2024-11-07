namespace Dfe.PlanTech.Domain.Caching.Models;

public record DistributedCachingOptions(
    string ConnectionString,
    int DistLockAcquisitionTimeoutInSeconds = 30,
    int DistLockMaxDurationInSeconds = 200)
{
    public bool HaveConnectionString => !string.IsNullOrEmpty(ConnectionString);
}
