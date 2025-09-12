using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CategoryLandingViewComponentViewModel
{
    public bool AllSectionsCompleted { get; init; }
    public bool AnySectionsCompleted { get; init; }
    public string CategoryName { get; set; } = null!;
    public ICollection<CategoryLandingSectionViewModel> CategoryLandingSections { get; init; } = null!;
    public string CategorySlug { get; set; } = null!;
    public string? NoSectionsErrorRedirectUrl { get; set; }
    public string? ProgressRetrievalErrorMessage { get; set; }
    public string? SectionName { get; set; }
    public ICollection<QuestionnaireSectionEntry> Sections { get; set; } = [];
}
