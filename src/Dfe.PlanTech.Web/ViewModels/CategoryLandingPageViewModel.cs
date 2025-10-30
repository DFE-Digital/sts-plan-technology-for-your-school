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
    public required ComponentTitleEntry Title { get; set; }
    public bool Print { get; set; }
}
