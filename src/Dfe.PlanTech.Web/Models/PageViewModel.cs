using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Models;

public class PageViewModel : BasePageViewModel
{
    public required Page Page { get; init; }
    public required string GTMHead { get; set; }
    public required string GTMBody { get; set; }
    public string Param { get; set; } = null!;
}
