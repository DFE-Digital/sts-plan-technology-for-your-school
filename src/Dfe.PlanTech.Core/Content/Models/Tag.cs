namespace Dfe.PlanTech.Domain.Questionnaire.Models;

public class Tag(string? text = null, string? colour = null)
{
    public string? Text { get; } = text;

    public string? Colour { get; } = colour;
}
