using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Web.ViewModels.Inputs;

public class SingleRecommendationInputViewModel : IValidatableObject
{
    public string? SelectedStatus { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public RecommendationStatus? SelectedStatusEnum =>
        SelectedStatus.GetRecommendationStatusEnumValue();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SelectedStatusEnum == null)
        {
            var error =
                $"Invalid / unrecognised status value received: {SelectedStatus}: {SelectedStatusEnum}";
            yield return new ValidationResult(error);
        }
    }

    public IEnumerable<ValidationResult> ValidateForWorkflow(RecommendationStatus? currentStatus)
    {
        if (currentStatus == SelectedStatusEnum && string.IsNullOrWhiteSpace(Notes))
        {
            yield return new ValidationResult(
                "No change to current data. Ignore and do not process."
            );
        }
    }

    public SingleRecommendationModel ToModel()
    {
        return new SingleRecommendationModel { SelectedStatus = SelectedStatusEnum, Notes = Notes };
    }
}
