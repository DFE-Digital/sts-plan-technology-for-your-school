using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class GridContainerRenderer : BaseRichTextContentPartRender, IRichTextContentPartRenderer
{
    public GridContainerRenderer() : base(RichTextNodeType.GridContainer)
    {
        
    }
    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        throw new NotImplementedException();
    }
}
