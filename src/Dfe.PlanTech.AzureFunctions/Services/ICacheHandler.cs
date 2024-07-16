namespace Dfe.PlanTech.AzureFunctions.Services;

public interface ICacheHandler
{
    Task RequestCacheClear(CancellationToken cancellationToken);
}
