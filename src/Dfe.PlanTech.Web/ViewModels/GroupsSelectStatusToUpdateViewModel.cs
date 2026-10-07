using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]

public class GroupsSelectStatusToUpdateViewModel : UpdateRecommendationViewModel
{
    public string? CategorySlug { get; set; }
    public QuestionnaireSectionEntry? Section { get; set; } = null!;

    public List<string>? SelectedSchoolsRefs { get; set; } = [];
    public List<string?> SelectedSchoolNames { get; set; } = [];
    public IEnumerable<string>? ErrorMessages { get; set; }

}
