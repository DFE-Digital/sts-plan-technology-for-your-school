using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Interfaces;

public interface IRichTextRenderer
{
    public string ToHtml(IRichTextContent content);
}
