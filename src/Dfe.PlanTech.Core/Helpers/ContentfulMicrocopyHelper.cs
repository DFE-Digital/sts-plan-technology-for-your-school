using Dfe.PlanTech.Core.Contentful.Models;
using static Dfe.PlanTech.Core.Constants.ContentfulMicrocopyConstants;

namespace Dfe.PlanTech.Core.Helpers;

public static class ContentfulMicrocopyHelper
{
    public static string GetMicrocopyTextByKey(
        MicrocopyRecord record,
        List<MicrocopyEntry>? microcopyEntries,
        Dictionary<string, string>? dynamicValues = null
    )
    {
        var microcopyText =
            microcopyEntries?.FirstOrDefault(r => r.Key == record.Key)?.Value
            ?? record.FallbackText;

        if (record.Variables is not [])
        {
            if (dynamicValues == null)
            {
                return record.FallbackText;
            }
            else
            {
                microcopyText = ReplaceVariables(microcopyText, record, dynamicValues);
            }
        }

        return microcopyText;
    }

    private static string ReplaceVariables(
        string microcopyText,
        MicrocopyRecord record,
        Dictionary<string, string> dynamicValues
    )
    {
        var text = microcopyText;
        foreach (var variable in record.Variables)
        {
            if (!dynamicValues.TryGetValue(variable, out var value) || value is null)
            {
                return record.FallbackText;
            }

            text = text.Replace($"{{{{{variable}}}}}", value);
        }
        return text;
    }
}
