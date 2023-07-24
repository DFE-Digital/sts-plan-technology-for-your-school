using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.Models
{
    public class RecommendationsViewModel
    {
        [Required]
        public string BackUrl { get; init; } = null!;
    }
}

