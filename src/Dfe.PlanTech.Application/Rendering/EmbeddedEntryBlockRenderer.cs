using System.Text;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Rendering;

public class EmbeddedEntryBlockRenderer(ILoggerFactory loggerFactory)
    : BaseRichTextContentPartRenderer(RichTextNodeType.EmbeddedEntryBlock)
{
    public override StringBuilder AddHtml(
        RichTextContentField content,
        IRichTextContentPartRendererCollection rendererCollection,
        StringBuilder stringBuilder
    )
    {
        var richTextData = content.Data?.Target ?? null;
        if (richTextData == null)
        {
            return stringBuilder;
        }

        switch (richTextData.SystemProperties.ContentType.SystemProperties.Id)
        {
            case ContentfulContentTypeConstants.ComponentAttachmentContentfulContentTypeId:
                return AttachmentComponentRenderer.AddHtml(content, stringBuilder);
            case ContentfulContentTypeConstants.ComponentAccordionContentfulContentTypeId:
                var accordionComponent = new AccordionComponentRenderer(
                    loggerFactory.CreateLogger<AccordionComponentRenderer>()
                );
                return accordionComponent.AddHtml(content, rendererCollection, stringBuilder);
            default:
                break;
        }

        return stringBuilder;
    }
}
