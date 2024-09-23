namespace Dfe.PlanTech.Application.Caching.Services;
public interface ICacheHandler
{
    Task RequestCacheClear(CancellationToken cancellationToken);
}
