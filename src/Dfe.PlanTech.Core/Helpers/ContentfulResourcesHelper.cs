using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Helpers;

public static class ContentfulResourcesHelper
{
    public static string GetCategoryLandingInsetText(string categoryName, int sectionsCompleted, List<ResourceEntry> resources)
    {
        var statusText = string.Empty;

        if (sectionsCompleted == 0)
        {
            statusText = resources.FirstOrDefault(r => r.Key == ContentfulResourceConstants.LandingPageInsetIntroNotStarted)?.Value
                ?? ContentfulResourceConstants.GetFallbackText(ContentfulResourceConstants.LandingPageInsetIntroNotStarted);                
        }
        else
        {
            statusText = resources.FirstOrDefault(r => r.Key == ContentfulResourceConstants.LandingPageInsetIntroContinue)?.Value
               ?? ContentfulResourceConstants.GetFallbackText(ContentfulResourceConstants.LandingPageInsetIntroContinue);
        }

        return statusText?.Replace("{{standard}}", categoryName) ?? string.Empty;
    }
}
