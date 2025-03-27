using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface ICardContainerContentPartRenderer
{
    public string ToHtml(List<CsCard>? content);
}
