namespace Dfe.PlanTech.Domain.Caching.Interfaces;

public interface ICacheHandler
{
    Task RequestCacheClear(CancellationToken cancellationToken);
}
