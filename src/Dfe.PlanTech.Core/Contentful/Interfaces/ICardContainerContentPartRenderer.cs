using Dfe.PlanTech.Core.DataTransferObjects.Contentful;

namespace Dfe.PlanTech.Core.Contentful.Interfaces;

public interface ICardContainerContentPartRenderer
{
    public string ToHtml(IReadOnlyList<CmsCardComponentDto>? content);
}
