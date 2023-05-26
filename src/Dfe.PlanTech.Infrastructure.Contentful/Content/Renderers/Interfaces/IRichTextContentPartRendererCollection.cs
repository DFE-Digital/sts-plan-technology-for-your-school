using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Interfaces;

public interface IRichTextContentPartRendererCollection
{
    public IRichTextContentPartRenderer? GetRendererForContent(IRichTextContent content);
}