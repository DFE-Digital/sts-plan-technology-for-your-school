using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Dfe.PlanTech.Web.ViewModels.Inputs;

[ExcludeFromCodeCoverage]
public class GroupSelectSchoolsToAssessInputViewModel
{
    [Required]
    public string SectionId { get; init; } = null!;

    public List<string>? SelectedSchoolsRefs {  get; set; } = [];
}

