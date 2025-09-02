using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CategoryLandingPageViewModel
{
    public QuestionnaireCategoryEntry Category { get; set; } = null!;
    public string? SectionName { get; set; }
    public string Slug { get; set; } = null!;
    public ComponentTitleEntry Title { get; set; } = null!;
}
