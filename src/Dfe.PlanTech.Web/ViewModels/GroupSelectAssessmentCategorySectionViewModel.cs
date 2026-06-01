namespace Dfe.PlanTech.Web.ViewModels;

public class CategorySectionViewModel
{
    public string? CategoryName { get; set; }
    public List<GroupSelectAssessmentSectionViewModel> Sections { get; set; } = [];
}
