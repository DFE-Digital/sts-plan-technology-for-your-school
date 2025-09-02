using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CategorySectionViewComponentViewModel
{
    public string CategoryHeaderText { get; set; } = null!;
    public List<CategorySectionViewModel> CategorySections { get; init; } = null!;
    public string? CategorySlug { get; set; }
    public int CompletedSectionCount { get; init; }
    public ContentfulEntry Description { get; set; } = null!;
    public string? NoSectionsErrorRedirectUrl { get; set; }
    public string? ProgressRetrievalErrorMessage { get; init; }
    public int TotalSectionCount { get; init; }
}
