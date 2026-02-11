using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using System.Resources;

namespace Dfe.PlanTech.Core.Helpers;

public static class ContentfulMicrocopyHelper
{
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
        else
        {
            throw new ArgumentException();
        }
    }
}
