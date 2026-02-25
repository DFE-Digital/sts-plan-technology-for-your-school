using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using static Dfe.PlanTech.Core.Constants.ContentfulMicrocopyConstants;

namespace Dfe.PlanTech.Core.Helpers;

public static class ContentfulMicrocopyHelper
{
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
