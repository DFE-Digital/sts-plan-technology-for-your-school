using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CategoryCardsViewComponentViewModel
{
    public required string CategoryHeaderText { get; set; }
    public string? CategorySlug { get; set; }
    public int CompletedSectionCount { get; init; }
    public required ContentfulEntry Description { get; set; }
    public string? NoSectionsErrorRedirectUrl { get; set; }
    public string? ProgressRetrievalErrorMessage { get; init; }
    public int TotalSectionCount { get; init; }
    public CategoryLandingContext Context { get; init; } = CategoryLandingContext.School;
    public bool IsMat => Context == CategoryLandingContext.MAT;
}
