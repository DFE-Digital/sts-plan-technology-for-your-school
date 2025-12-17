using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Core.Contentful.Interfaces;

public interface ICardContainerContentPartRenderer
{
    public string ToHtml(IReadOnlyList<ComponentCardEntry>? content);
}
