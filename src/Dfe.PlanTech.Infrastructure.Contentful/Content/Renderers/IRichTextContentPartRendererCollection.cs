using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content;

public interface IRichTextContentPartRendererCollection
{
    public IRichTextContentPartRenderer? GetRendererForContent(IRichTextContent content);
}