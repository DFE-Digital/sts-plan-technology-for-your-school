using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Web.ViewModels;

public class CategoryCardsViewComponentViewModel
{
    public required string CategoryHeaderText { get; set; }
    public string? CategorySlug { get; set; }
    public int CompletedSectionCount { get; init; }
    public required ContentfulEntry Description { get; set; }
    public string? NoSectionsErrorRedirectUrl { get; set; }
    public string? ProgressRetrievalErrorMessage { get; init; }
    public int TotalSectionCount { get; init; }
    public required IEnumerable<ResourceEntry> Resources { get; init; }
    public string StatusText => GetStatusText();

    private string GetStatusText()
    {
        string intendedText;

        if (CompletedSectionCount == 0 && TotalSectionCount == 1)
        {
            intendedText = ContentfulResourceConstants.HomeCardStatusSingleNotStarted;
        }
        else if (CompletedSectionCount == 0)
        {
            intendedText = ContentfulResourceConstants.HomeCardStatusMultipleNotStarted;
        }
        else
        {
            intendedText = CompletedSectionCount < TotalSectionCount
                ? ContentfulResourceConstants.HomeCardStatusContinue
                : ContentfulResourceConstants.HomeCardStatusViewRecommendations;
        }

        return Resources
            .FirstOrDefault(r => r.Key == intendedText)?.Value
            ?? ContentfulResourceConstants.GetFallbackText(intendedText);
    }
}
