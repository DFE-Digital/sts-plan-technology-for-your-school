using Dfe.PlanTech.Domain.Content.Models;
using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.Models;

public class CookiesViewModel
{
    [Required]
    public Title Title { get; init; } = null!;

    [Required]
    public List<ContentComponent> Content { get; init; } = null!;

}