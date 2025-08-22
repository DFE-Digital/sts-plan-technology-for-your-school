using System.Text;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Rendering;

public class EmbeddedEntryBlockRenderer(
    ILoggerFactory loggerFactory
) : BaseRichTextContentPartRenderer(RichTextNodeType.EmbeddedEntryBlock)
{
    public override StringBuilder AddHtml(RichTextContentField content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        var richTextData = content.Data?.Target ?? null;
        if (richTextData == null)
        {
            return stringBuilder;
        }

        switch (richTextData.SystemProperties.ContentType.SystemProperties.Id)
        {
            case ContentTypeConstants.ComponentAttachmentContentTypeId:
                var attachment = new AttachmentComponentRenderer();
                return attachment.AddHtml(content, stringBuilder);
            case ContentTypeConstants.ComponentAccordionContentTypeId:
                var accordionComponent = new AccordionComponentRenderer(loggerFactory.CreateLogger<AccordionComponentRenderer>());
                return accordionComponent.AddHtml(content, rendererCollection, stringBuilder);
            default:
                break;
        }

        return stringBuilder;
    }
}
