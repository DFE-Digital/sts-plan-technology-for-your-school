using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.Models;

public class CookiesViewModel
{
    [Required]
    public Title Title { get; init; } = null!;

    [Required]
    public IContentComponent[] Content { get; init; } = null!;

}