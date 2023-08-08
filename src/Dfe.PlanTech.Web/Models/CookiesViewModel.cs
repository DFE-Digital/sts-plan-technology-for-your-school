using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Models
{
    public class CookiesViewModel
    {
        [Required]
        public string BackUrl { get; init; } = null!;
        
        [Required]
        public Title Title { get; init; } = null!;
        
        [Required]
        public IContentComponent[] Content { get; init; } = null!;
        
    }
}