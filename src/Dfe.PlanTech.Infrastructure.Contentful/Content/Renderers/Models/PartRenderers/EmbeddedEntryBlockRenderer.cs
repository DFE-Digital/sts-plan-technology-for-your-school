using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class EmbeddedEntryBlockRenderer : BaseRichTextContentPartRender, IRichTextContentPartRendererCollection
{
    public ILogger Logger { get; }
    public IReadOnlyList<IRichTextContentPartRenderer> Renders { get; private set; }
    public EmbeddedEntryBlockRenderer() : base(RichTextNodeType.EmbeddedEntryBlock)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        var richTextData = content?.Data?.Target ?? null;
        if (richTextData != null)
        {
            switch (richTextData.SystemProperties.ContentType?.SystemProperties.Id)
            {
                case "Attachment":
                    var attachment = new AttachmentComponent();
                    return attachment.AddHtml(content, stringBuilder);
                case "CSAccordion":
                    var accordionComponent = new AccordionComponent(Logger);
                    return accordionComponent.AddHtml(content, rendererCollection, stringBuilder);
                default:
                    break;
            }
        }

        return stringBuilder;
    }

    public void RenderChildren(RichTextContent content, StringBuilder stringBuilder)
    {
        foreach (var subContent in content.Content)
        {
            var renderer = GetRendererForContent(subContent);

            if (renderer == null)
            {
                //_logger.LogWarning("Could not find renderer for {subContent}", subContent);
                continue;
            }

            renderer.AddHtml(subContent, this, stringBuilder);
        }
    }

    public IRichTextContentPartRenderer? GetRendererForContent(RichTextContent content)
    => Renders.FirstOrDefault(renderer => renderer.Accepts(content));

}
