using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]

public class GroupsSelectStatusToUpdateViewModel
{
    public string? CategorySlug { get; set; }
    public string SectionSlug { get; set; } = string.Empty;
    public QuestionnaireSectionEntry? Section { get; set; } = null!;
    public RecommendationChunkEntry CurrentChunk { get; set; } = null!;

    public List<string>? SelectedSchoolsRefs { get; set; } = [];
    public List<string?> SelectedSchoolNames { get; set; } = [];
    public IEnumerable<string>? ErrorMessages { get; set; }

    public int RecommendationId { get; set; }
    public string? StatusErrorMessage { get; set; }
    public IDictionary<RecommendationStatus, string> StatusOptions { get; set; } =
        new Dictionary<RecommendationStatus, string>();
    public required RecommendationStatus SelectedStatusKey { get; init; } = RecommendationStatus.NotStarted;

}
