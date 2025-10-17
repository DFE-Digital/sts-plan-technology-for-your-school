using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Core.Enums;

public enum RecommendationStatus
{
    // Name strings use the character code "\u00A0" instead of space and not HTML entity "&nbsp;" to avoid Html.Raw in Razor views

    [Display(Name = "Not started", Description = "govuk-tag--grey")]
    NotStarted = 0,

    [Display(Name = "In progress", Description = "govuk-tag--blue")]
    InProgress = 1,

    [Display(Name = "On hold", Description = "govuk-tag--yellow")]
    OnHold = 2,

    [Display(Name = "Complete", Description = "govuk-tag--green")]
    Complete = 3
}
