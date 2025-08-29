using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Contentful.Models;

[ExcludeFromCodeCoverage]
public class QuestionnaireCategoryEntry : ContentfulEntry
{
    public string InternalName { get; set; } = "";
    public ComponentHeaderEntry Header { get; set; } = null!;
    public List<ContentfulEntry>? Content { get; set; }
    public List<QuestionnaireSectionEntry> Sections { get; set; } = [];
    public PageEntry? LandingPage { get; set; }
}
