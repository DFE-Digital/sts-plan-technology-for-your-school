using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CategoryLandingViewComponentViewModel
{
    public int CompletedSectionCount { get; init; }
    public required string CategoryName { get; set; }
    public ICollection<CategoryLandingSectionViewModel> CategoryLandingSections { get; init; } = [];
    public required string CategorySlug { get; set; }
    public string? NoSectionsErrorRedirectUrl { get; set; }
    public string? ProgressRetrievalErrorMessage { get; set; }
    public string? SectionName { get; set; }
    public ICollection<QuestionnaireSectionEntry> Sections { get; set; } = [];
    public RecommendationSortOrder? SortType { get; set; }
    public bool Print { get; set; }
    public required string StatusLinkPartialName { get; set; }
    public List<ResourceEntry> Resources { get; set; } = [];
}
