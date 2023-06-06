using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Domain.Content.Interfaces;

public interface IRichTextContentPartRendererCollection
{
    public IRichTextContentPartRenderer? GetRendererForContent(IRichTextContent content);
}