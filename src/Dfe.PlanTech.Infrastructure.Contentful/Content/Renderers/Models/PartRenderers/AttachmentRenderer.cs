
using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class AttachmentRenderer : BaseRichTextContentPartRender
{
    public AttachmentRenderer() : base(RichTextNodeType.EmbeddedEntryBlock)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {

        rendererCollection.RenderChildren(content, stringBuilder);

        return stringBuilder;
    }

    private CustomAttachment GenerateCustomAttachment(Target target)
    {
        return new CustomAttachment
        {
            InternalName = target.InternalName,
            ContentType = target.Asset.File.ContentType,
            Size = target.Asset.File.Details.Size,
            Title = target.Title,
            Uri = target.Asset.File.Url,
            UpdatedAt = target.Asset.SystemProperties.UpdatedAt
        };
    }
}
