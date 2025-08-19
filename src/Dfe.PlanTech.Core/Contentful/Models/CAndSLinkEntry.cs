namespace Dfe.PlanTech.Core.Contentful.Models;

public class CAndSLinkEntry : ContentfulEntry
{
    public string InternalName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string LinkText { get; set; } = null!;
}
