namespace Dfe.PlanTech.Core.Helpers;

public static class TagColourHelper
{
    public static readonly string Default = "blue";
    public static readonly string Blue = "blue";
    public static readonly string Grey = "grey";
    public static readonly string LightBlue = "light-blue";
    public static readonly string Red = "red";
    public static readonly string Green = "green";
    public static readonly string Yellow = "yellow";

    private static readonly string[] _colours = [Blue, Grey, LightBlue, Red, Green, Yellow];

    public static string GetMatchingColour(string? toMatch) =>
        string.IsNullOrEmpty(toMatch)
            ? Default
            : _colours.FirstOrDefault(
                colour =>
                    string.Equals(colour, toMatch, StringComparison.InvariantCultureIgnoreCase),
                Default
            );
}
