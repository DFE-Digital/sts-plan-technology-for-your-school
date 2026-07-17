using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class GroupsSelectSchoolsToAssessViewModel
{
    public string? CategorySlug { get; set; }
    public QuestionnaireSectionEntry? Section { get; set; } = null!;
    public List<SubmissionInformationModel>? SchoolSubmissionInfo { get; set; } = null!;
    public IEnumerable<string>? ErrorMessages { get; set; }
    public List<string> PresentedSchoolRefs { get; set; } = [];
    public List<string>? SelectedSchoolsRefs { get; set; } = [];
}
