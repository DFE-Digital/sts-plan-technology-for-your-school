
using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;
using Microsoft.Extensions.Primitives;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class AttachmentRenderer : BaseRichTextContentPartRender
{
    public AttachmentRenderer() : base(RichTextNodeType.EmbeddedEntryBlock)
    {
    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        var asset = content.Data.Target.Asset;
        var file = asset.File;
        var uri = file.Url;
        var title = asset.Title;
        stringBuilder.Append("<div class=\"attachment-details\">");
        stringBuilder.Append("<div class=\"attachment-title\">");
        //stringBuilder.Append($"<a href=\"/{uri}/" aria-describedby=\"file-details\" class=\"govuk-link attachment-link\" download>{title}</a>");

        stringBuilder.Append("<a href=\"");
        stringBuilder.Append(uri);
        stringBuilder.Append($"\"aria-describedby=\"file-details\" class=\"govuk-link attachment-link\" download>{title}");
        stringBuilder.Append("</a>");

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
