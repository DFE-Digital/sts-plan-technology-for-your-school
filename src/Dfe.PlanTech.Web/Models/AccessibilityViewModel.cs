using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Models
{
    public class AccessibilityViewModel
    {
        [Required]
        public Title Title { get; init; } = null!;
        
        [Required]
        public IContentComponent[] Content { get; init; } = null!;
        
        [Required]
        public bool UserIsAuthenticated { get; init; }
    }
}