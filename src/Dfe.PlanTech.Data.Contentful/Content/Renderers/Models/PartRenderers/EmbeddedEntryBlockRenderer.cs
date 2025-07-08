using System.Text;
using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Data.Contentful.Content.Renderers.Models.PartRenderers;

public class EmbeddedEntryBlockRenderer : BaseRichTextContentPartRender
{
    private readonly ILogger<EmbeddedEntryBlockRenderer> _logger;
    private readonly ILogger<AccordionComponent> _loggerAccordion;
    public EmbeddedEntryBlockRenderer(ILoggerFactory loggerFactory, ILogger<EmbeddedEntryBlockRenderer> logger) : base(RichTextNodeType.EmbeddedEntryBlock)
    {
        _logger = logger;
        _loggerAccordion = loggerFactory.CreateLogger<AccordionComponent>();
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        var richTextData = content.Data?.Target ?? null;
        if (richTextData == null)
        {
            return stringBuilder;
        }

        switch (richTextData.SystemProperties.ContentType?.SystemProperties.Id)
        {
            case ContentTypeId.Attachment:
                var attachment = new AttachmentComponent();
                return attachment.AddHtml(content, stringBuilder);
            case ContentTypeId.Accordion:
                var accordionComponent = new AccordionComponent(_loggerAccordion);
                return accordionComponent.AddHtml(content, rendererCollection, stringBuilder);
            default:
                break;
        }

        return stringBuilder;
    }
}
