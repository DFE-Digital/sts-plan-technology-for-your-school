namespace Dfe.PlanTech.Core.Caching;

public record DistributedCachingOptions(
    string ConnectionString,
    int DistLockAcquisitionTimeoutInSeconds = 30,
    int DistLockMaxDurationInSeconds = 200)
{
    public bool HaveConnectionString => !string.IsNullOrEmpty(ConnectionString);
}
