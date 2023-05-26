using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Interfaces;

public interface IRichTextContentPartRenderer
{
    public bool Accepts(IRichTextContent content);

    public StringBuilder AddHtml(IRichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder);
}