using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using static Dfe.PlanTech.Core.Constants.ContentfulMicrocopyConstants;

namespace Dfe.PlanTech.Core.Helpers;

public static class ContentfulMicrocopyHelper
{
    public static string GetCategoryLandingInsetText(string categoryName, int sectionsCompleted, List<MicrocopyEntry>? microcopy)
    {
        var statusText = string.Empty;

        if (sectionsCompleted == 0)
        {
            statusText = microcopy?.FirstOrDefault(r => r.Key == ContentfulMicrocopyConstants.LandingPageInsetIntroNotStarted.Key)?.Value
                ?? ContentfulMicrocopyHelper.GetFallbackText(ContentfulMicrocopyConstants.LandingPageInsetIntroNotStarted.Key);
        }
        else
        {
            statusText = microcopy?.FirstOrDefault(r => r.Key == ContentfulMicrocopyConstants.LandingPageInsetIntroContinue.Key)?.Value
               ?? ContentfulMicrocopyHelper.GetFallbackText(ContentfulMicrocopyConstants.LandingPageInsetIntroContinue.Key);
        }

        return statusText?.Replace("{{standard}}", categoryName) ?? string.Empty;
    }

    public static string GetCardStatusText(int completedSectionCount, int totalSectionCount, List<MicrocopyEntry>? microcopyEntries)
    {
        string intendedText;

        if (completedSectionCount == 0 && totalSectionCount == 1)
        {
            intendedText = ContentfulMicrocopyConstants.HomeCardStatusSingleNotStarted.Key;
        }
        else if (completedSectionCount == 0)
        {
            intendedText = ContentfulMicrocopyConstants.HomeCardStatusMultipleNotStarted.Key;
        }
        else
        {
            intendedText = completedSectionCount < totalSectionCount
                ? ContentfulMicrocopyConstants.HomeCardStatusContinue.Key
                : ContentfulMicrocopyConstants.HomeCardStatusViewRecommendations.Key;
        }

        return microcopyEntries?.FirstOrDefault(r => r.Key == intendedText)?.Value
            ?? GetFallbackText(intendedText);
    }

    public static string GetMicrocopyTextByKey(
        MicrocopyRecord record,
        List<MicrocopyEntry>? microcopyEntries,
        Dictionary<string, string>? dynamicValues = null)
    {

        var microcopyText = microcopyEntries?.FirstOrDefault(r => r.Key == record.Key)?.Value
            ?? GetFallbackText(record.Key);

        if (dynamicValues != null)
        {
            microcopyText = ReplaceVariables(microcopyText, record, dynamicValues);
        }

        return microcopyText;
    }

    private static string ReplaceVariables(string microcopyText, MicrocopyRecord record, Dictionary<string, string> dynamicValues)
    {
        var text = microcopyText;
        foreach (var variable in record.Variables)
        {
            if (!dynamicValues.TryGetValue(variable, out var value) || value is null)
            {
                return GetFallbackText(record.Key);
            }

            text = text.Replace($"{{{{{variable}}}}}", value);
        }
        return text;
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
