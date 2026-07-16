using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Providers.Interfaces
{
    public interface IBannerConditionsContextProvider
    {
        Task<bool> RecordViewActionAndGetBannerVisibility(ComponentNotificationBannerEntry banner);
    }
}
