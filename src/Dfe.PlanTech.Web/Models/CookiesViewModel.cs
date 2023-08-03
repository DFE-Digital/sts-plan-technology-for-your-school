using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.Models
{
    public class CookiesViewModel
    {
        [Required]
        public string BackUrl { get; init; } = null!;
    }
}