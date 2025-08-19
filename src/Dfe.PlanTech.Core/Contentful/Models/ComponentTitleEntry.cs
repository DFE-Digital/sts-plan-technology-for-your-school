namespace Dfe.PlanTech.Core.Contentful.Models;

public class ComponentTitleEntry : ContentfulEntry

{
    public string InternalName { get; set; } = null!;
    public string Text { get; init; } = null!;

    public ComponentTitleEntry(string title)
    {
        Text = title;
    }
}
