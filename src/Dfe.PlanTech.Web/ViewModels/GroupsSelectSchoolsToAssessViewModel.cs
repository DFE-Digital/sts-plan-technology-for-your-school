using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class GroupsSelectSchoolsToAssessViewModel
{
    public string? CategorySlug { get; init; }
    public required QuestionnaireSectionEntry Section { get; init; }
    public required List<SubmissionInformationModel> SchoolSubmissionInfo { get; init; }
    public IEnumerable<string>? ErrorMessages { get; set; }
    public List<string>? SelectedSchoolsRefs { get; set; } = [];
}
