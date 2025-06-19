using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Cookie;

namespace Dfe.PlanTech.Web.Models;

public class CookiesViewModel
{
    public Title Title { get; init; } = null!;

    public List<ContentComponent> Content { get; init; } = null!;

    public DfeCookie Cookie { get; init; } = new DfeCookie();

    public string ReferrerUrl { get; init; } = null!;
}
