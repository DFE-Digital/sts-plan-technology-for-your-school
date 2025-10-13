using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Core.Enums;

public enum RecommendationSort
{
    [Display(Name="Default")]
    Default = 0,

    [Display(Name = "Status")]
    Status = 1,

    [Display(Name = "Last updated")]
    LastUpdated = 2
}
