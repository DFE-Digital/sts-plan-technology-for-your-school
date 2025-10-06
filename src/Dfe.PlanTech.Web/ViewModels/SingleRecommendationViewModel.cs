using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class SingleRecommendationViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string SectionSlug { get; set; } = string.Empty;
    public QuestionnaireSectionEntry Section { get; set; } = null!;
    public List<RecommendationChunkEntry> Chunks { get; set; } = [];
    public RecommendationChunkEntry CurrentChunk { get; set; } = null!;
    public RecommendationChunkEntry? PreviousChunk { get; set; } = null!;
    public RecommendationChunkEntry? NextChunk { get; set; } = null!;
    public int CurrentChunkPosition { get; set; }
    public int TotalChunks { get; set; }
    public required string Status { get; init; } // TODO: Enum +- static constants `Dfe.PlanTech.Core.Constants.RecommendationConstants`
    public string StatusText => Status;
    public string StatusTagClass => Status switch // TODO: centralise, as this logic will be shared site-wide (maybe just pull out the colour?)
    {
        "Complete" => "govuk-tag--green",
        "In progress" => "govuk-tag--blue",
        "On hold" => "govuk-tag--red",
        "Not started" => "govuk-tag--grey",
        _ => "govuk-tag--grey"
    };
    public required DateTime? LastUpdated { get; init; }
    public string LastUpdatedFormatted => LastUpdated?.ToString("d MMMM yyyy") ?? string.Empty; // TODO: Consider a default along the lines of "not known" / "never updated"?
}
