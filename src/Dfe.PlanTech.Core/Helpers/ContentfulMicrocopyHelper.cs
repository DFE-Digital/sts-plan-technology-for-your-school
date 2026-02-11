using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Helpers;

public static class ContentfulMicrocopyHelper
{
    public static string GetCategoryLandingInsetText(string categoryName, int sectionsCompleted, List<MicrocopyEntry>? microcopy)
    {
        var statusText = string.Empty;

        if (sectionsCompleted == 0)
        {
            statusText = microcopy?.FirstOrDefault(r => r.Key == ContentfulMicrocopyConstants.LandingPageInsetIntroNotStarted)?.Value
                ?? ContentfulMicrocopyHelper.GetFallbackText(ContentfulMicrocopyConstants.LandingPageInsetIntroNotStarted);
        }
        else
        {
            statusText = microcopy?.FirstOrDefault(r => r.Key == ContentfulMicrocopyConstants.LandingPageInsetIntroContinue)?.Value
               ?? ContentfulMicrocopyHelper.GetFallbackText(ContentfulMicrocopyConstants.LandingPageInsetIntroContinue);
        }

        return statusText?.Replace("{{standard}}", categoryName) ?? string.Empty;
    }

    public static string GetCardStatusText(int completedSectionCount, int totalSectionCount, List<MicrocopyEntry>? microcopyEntries)
    {
        string intendedText;

        if (completedSectionCount == 0 && totalSectionCount == 1)
        {
            intendedText = ContentfulMicrocopyConstants.HomeCardStatusSingleNotStarted;
        }
        else if (completedSectionCount == 0)
        {
            intendedText = ContentfulMicrocopyConstants.HomeCardStatusMultipleNotStarted;
        }
        else
        {
            intendedText = completedSectionCount < totalSectionCount
                ? ContentfulMicrocopyConstants.HomeCardStatusContinue
                : ContentfulMicrocopyConstants.HomeCardStatusViewRecommendations;
        }

        return microcopyEntries?.FirstOrDefault(r => r.Key == intendedText)?.Value
            ?? GetFallbackText(intendedText);
    }

    public static string GetFallbackText(string intendedText)
    {
        if (ContentfulMicrocopyConstants.EmptyFallback.Contains(intendedText))
        {
            return string.Empty;
        }
        else if (ContentfulMicrocopyConstants.CardsFallback.Contains(intendedText))
        {
            return "Go to standard";
        }
        else if (ContentfulMicrocopyConstants.TopicLinksFallback.Contains(intendedText))
        {
            return "Start, continue or view answers for this self-assessment";
        }
        else if (ContentfulMicrocopyConstants.PrintLinksFallback.Contains(intendedText))
        {
            return "Print all recommendations";
        }
        else
        {
            throw new ArgumentException();
        }
    }
}
