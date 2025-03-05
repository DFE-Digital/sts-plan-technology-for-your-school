using System.Text;
using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class EmbeddedEntryBlockRenderer : BaseRichTextContentPartRender, IRichTextContentPartRendererCollection
{
    private readonly ILogger<EmbeddedEntryBlockRenderer> _logger;
    private readonly ILogger<AccordionComponent> _loggerAccordion;
    public ILogger Logger => _logger;
    public IReadOnlyList<IRichTextContentPartRenderer> Renders { get; private set; }
    public EmbeddedEntryBlockRenderer(ILoggerFactory loggerFactory, ILogger<EmbeddedEntryBlockRenderer> logger) : base(RichTextNodeType.EmbeddedEntryBlock)
    {
        _logger = logger;
        _loggerAccordion = loggerFactory.CreateLogger<AccordionComponent>();
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        var richTextData = content?.Data?.Target ?? null;
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

    public void RenderChildren(RichTextContent content, StringBuilder stringBuilder)
    {
        foreach (var subContent in content.Content)
        {
            var renderer = GetRendererForContent(subContent);

            if (renderer == null)
            {
                _logger.LogWarning("Could not find renderer for {subContent}", subContent);
                continue;
            }

            renderer.AddHtml(subContent, this, stringBuilder);
        }
    }

    public IRichTextContentPartRenderer? GetRendererForContent(RichTextContent content)
    => Renders.FirstOrDefault(renderer => renderer.Accepts(content));

}
