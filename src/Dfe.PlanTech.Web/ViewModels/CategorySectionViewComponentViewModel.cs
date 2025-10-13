using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CategorySectionViewComponentViewModel
{
    public required string CategoryHeaderText { get; set; }
    public List<CategorySectionViewModel> CategorySections { get; init; } = [];
    public string? CategorySlug { get; set; }
    public int CompletedSectionCount { get; init; }
    public required ContentfulEntry Description { get; set; }
    public string? NoSectionsErrorRedirectUrl { get; set; }
    public string? ProgressRetrievalErrorMessage { get; init; }
    public int TotalSectionCount { get; init; }
}
