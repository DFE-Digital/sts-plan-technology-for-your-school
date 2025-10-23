using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CategoryLandingPageViewModel
{
    public required QuestionnaireCategoryEntry Category { get; set; }
    public string? SectionName { get; set; }
    public required string Slug { get; set; }
    public string? SortOrder { get; set; }
    public List<ContentfulEntry> BeforeTitleContent { get; init; } = [];
    public required ComponentTitleEntry Title { get; set; }
}
