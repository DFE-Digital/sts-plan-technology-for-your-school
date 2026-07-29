using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.ViewModels.QaVisualiser;

namespace Dfe.PlanTech.Web.ViewModels;

public class GroupSelectAssessmentViewModel
{
    public string? GroupName { get; set; }
    public List<CategorySectionViewModel> Categories { get; set; } = [];
}


