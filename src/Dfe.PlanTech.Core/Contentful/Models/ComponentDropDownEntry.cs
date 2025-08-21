namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentDropDownEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public RichTextContentField? Content { get; set; } = null!;
}
