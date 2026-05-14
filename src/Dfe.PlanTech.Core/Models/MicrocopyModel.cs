namespace Dfe.PlanTech.Core.Models;

public class MicrocopyModel(
    string key,
    string value,
    string fallbackText,
    params string[] variables
    )    
{
    public string Key { get; init; } = key;
    public string Value { get; init; } = value;
    public string FallbackText { get; init; } = fallbackText;
    public string[] Variables { get; init; } = variables;

    private string ValueOrFallback => Value ?? FallbackText;

    public string GetText(Dictionary<string, string>? dynamicValues = null)
    {
        if (Variables.Length == 0)
        {
            return ValueOrFallback;
        }

        return dynamicValues == null
            ? FallbackText
            : ReplaceVariables(dynamicValues);
    }

    private string ReplaceVariables(Dictionary<string, string> dynamicValues)
    {
        var text = ValueOrFallback;

        foreach (var variable in Variables)
        {
            if (!dynamicValues.TryGetValue(variable, out var actualValue) || string.IsNullOrWhiteSpace(actualValue))
            {
                return FallbackText;
            }

            text = text.Replace($"{{{{{variable}}}}}", actualValue);
        }
        return text;
    }
}

