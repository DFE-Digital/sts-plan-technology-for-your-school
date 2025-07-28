using System.Text;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.DataTransferObjects.Contentful;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Rendering;

public class EmbeddedEntryBlockRenderer(
    ILoggerFactory loggerFactory
) : BaseRichTextContentPartRenderer(RichTextNodeType.EmbeddedEntryBlock)
{
    public override StringBuilder AddHtml(CmsRichTextContentDto content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        var richTextData = content.Data?.Target ?? null;
        if (richTextData == null)
        {
            return stringBuilder;
        }

        switch (richTextData.Sys.ContentType)
        {
            case ContentTypeConstants.Attachment:
                var attachment = new AttachmentComponentRenderer();
                return attachment.AddHtml(content, stringBuilder);
            case ContentTypeConstants.Accordion:
                var accordionComponent = new AccordionComponent(loggerFactory);
                return accordionComponent.AddHtml(content, rendererCollection, stringBuilder);
            default:
                break;
        }

        return stringBuilder;
    }
}
