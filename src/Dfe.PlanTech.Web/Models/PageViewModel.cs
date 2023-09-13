using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Models;

public class PageViewModel
{
    public String OrganisationName { get; set; } = null!;
    public required Page Page { get; init; }
    public string Param { get; set; } = null!;
}
