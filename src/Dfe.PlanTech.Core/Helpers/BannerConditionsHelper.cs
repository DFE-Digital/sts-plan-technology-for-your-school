using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Core.Helpers;

public static class BannerConditionsHelper
{
    public static bool ShouldShowBanner(
        ComponentNotificationBannerEntry banner,
        BannerConditionsContextModel? status
    )
    {
        if (banner.DisplayFrom.HasValue && banner.DisplayFrom.Value > DateTime.UtcNow)
        {
            return false; // Banner is not yet active
        }
        if (banner.DisplayTo.HasValue && banner.DisplayTo.Value < DateTime.UtcNow)
        {
            return false; // Banner has expired
        }

        if (banner.Conditions == null || !banner.Conditions.Any())
        {
            return true; // No conditions, show the banner
        }
        foreach (var condition in banner.Conditions)
        {
            //if (status == "Unknown" && condition.ShowIfStatusUnknown == true)
            //{
            //    return true;
            //}
            //if (status == "NotStarted" && condition.ShowIfNotStarted == true)
            //{
            //    return true;
            //}
            //if (status == "InProgress" && condition.ShowIfInProgress == true)
            //{
            //    return true;
            //}
            //if (status == "Completed" && condition.ShowIfCompleted == true)
            //{
            //    return true;
            //}
        }
        return false; // No conditions matched, do not show the banner
    }
}
