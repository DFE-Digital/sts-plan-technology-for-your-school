using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.Attributes;

namespace Dfe.PlanTech.Core.Enums;

public enum RecommendationStatus
{
    // Name strings use the character code "\u00A0" instead of space and not HTML entity "&nbsp;" to avoid Html.Raw in Razor views

    [Display(Name = "Not\u00A0started")]
    [CssClass(ClassName = "govuk-tag--grey")]
    NotStarted = 0,

    [Display(Name = "In\u00A0progress")]
    [CssClass(ClassName = "govuk-tag--blue")]
    InProgress = 1,

    //[Display(Name = "On\u00A0hold")]
    //[CssClass(ClassName = "govuk-tag--red")]
    //OnHold = 2,

    // Legacy enum value
    [Display(Name = "Complete")]
    [CssClass(ClassName = "govuk-tag--green")]
    Complete = 3,
}
