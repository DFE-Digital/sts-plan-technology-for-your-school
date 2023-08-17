using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Web.Models;

public abstract class BasePageViewModel
{
  public List<NavigationLink> Links { get; set; } = new List<NavigationLink>();
}
