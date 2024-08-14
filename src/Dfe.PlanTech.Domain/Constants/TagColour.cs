namespace Dfe.PlanTech.Domain.Constants;

public static class TagColour
{
    public readonly static string Default = "blue";
    public readonly static string Blue = "blue";
    public readonly static string Grey = "grey";
    public readonly static string LightBlue = "light-blue";
    public readonly static string Red = "red";
    public readonly static string Green = "green";
    public readonly static string Yellow = "yellow";

    private readonly static string[] _colours = [Blue, Grey, LightBlue, Red, Green, Yellow];

    public static string GetMatchingColour(string? toMatch)
    => string.IsNullOrEmpty(toMatch) ?
        Default :
        _colours.FirstOrDefault(colour => string.Equals(colour, toMatch, StringComparison.InvariantCultureIgnoreCase), Default);
}
