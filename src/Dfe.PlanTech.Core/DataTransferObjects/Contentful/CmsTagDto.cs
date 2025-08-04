namespace Dfe.PlanTech.Core.DataTransferObjects.Contentful;

public class CmsTagDto
{
    public string? Text { get; }

    public string? Colour { get; }

    public CmsTagDto(string? text = null, string? colour = null)
    {
        Text = text;
        Colour = colour;
    }
}
