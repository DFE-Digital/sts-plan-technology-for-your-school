using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Core.Helpers;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class SingleRecommendationViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string SectionSlug { get; set; } = string.Empty;
    public string? OriginatingSlug { get; set; }

    public string? SuccessMessageTitle { get; set; }
    public string? SuccessMessageBody { get; set; }
    public string? StatusErrorMessage { get; set; }

    public QuestionnaireSectionEntry Section { get; set; } = null!;
    public List<RecommendationChunkEntry> Chunks { get; set; } = [];
    public RecommendationChunkEntry CurrentChunk { get; set; } = null!;
    public RecommendationChunkEntry? PreviousChunk { get; set; } = null!;
    public RecommendationChunkEntry? NextChunk { get; set; } = null!;
    public int CurrentChunkPosition { get; set; }
    public int TotalChunks { get; set; }

    public IDictionary<RecommendationStatus, string> StatusOptions { get; set; } =
        new Dictionary<RecommendationStatus, string>();
    public required RecommendationStatus SelectedStatusKey { get; init; }
    public required DateTime? LastUpdated { get; init; }
    public Dictionary<
        string,
        IEnumerable<SqlEstablishmentRecommendationHistoryDto>
    > History { get; init; } = [];

    public SqlFirstActivityForEstablishmentRecommendationDto? FirstActivity { get; init; }

    public string LastUpdatedFormatted =>
        LastUpdated?.ToString("d MMMM yyyy") ?? RecommendationConstants.DefaultLastUpdatedText;

    public string StatusText =>
        SelectedStatusKey.GetDisplayName();
    public string StatusTagClass =>
        ((RecommendationStatus?)SelectedStatusKey)
            .GetCssClassOrDefault(RecommendationConstants.DefaultTagClass);
}
