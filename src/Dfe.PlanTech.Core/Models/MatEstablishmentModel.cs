using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class MatEstablishmentModel
{
    public bool IsMatMultiSchoolAssessment { get; set; }
    public int SelectedSchoolCount { get; set; }
    public List<string> SelectedSchoolNames { get; set; } = [];

    public MatEstablishmentModel(
        bool isMatMultiSchoolAssessment,
        int selectedSchoolCount,
        List<string> selectedSchoolNames
    )
    {
        IsMatMultiSchoolAssessment = isMatMultiSchoolAssessment;
        SelectedSchoolCount = selectedSchoolCount;
        SelectedSchoolNames = selectedSchoolNames;
    }
}
