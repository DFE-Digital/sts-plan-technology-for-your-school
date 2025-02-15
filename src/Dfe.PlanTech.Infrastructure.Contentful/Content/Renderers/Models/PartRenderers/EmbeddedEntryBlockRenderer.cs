
using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class EmbeddedEntryBlockRenderer : BaseRichTextContentPartRender, IRichTextContentPartRendererCollection
{
    public ILogger Logger { get; }
    public IList<IRichTextContentPartRenderer> Renders { get; private set; }
    public EmbeddedEntryBlockRenderer() : base(RichTextNodeType.EmbeddedEntryBlock)
    {

    }

    public override StringBuilder AddHtml(RichTextContent content, IRichTextContentPartRendererCollection rendererCollection, StringBuilder stringBuilder)
    {
        Renders = rendererCollection.Renders;

        var nestedContent = content?.Data?.Target?.Content ?? null;
        if (nestedContent != null && nestedContent.Any())
        {
            foreach (var innerContent in nestedContent)
            {

                RenderChildren(innerContent.RichText, stringBuilder);
                return stringBuilder;

            }

        }

        var target = content?.Data?.Target ?? null;

        if (target != null)
        {
            var customAttachment = GenerateCustomAttachment(content?.Data?.Target);
            var uri = customAttachment.Uri;
            var title = customAttachment.Title;
            var fileExtension = customAttachment.ContentType.Split('/')[^1].ToLower();
            if (fileExtension == "vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                fileExtension = "xlsx";
            }
            stringBuilder.Append("<div class=\"guidance-container govuk-!-padding-8 govuk-!-margin-bottom-8 govuk-!-display-none-print\">");
            stringBuilder.Append("<div class=\"attachment\">");
            stringBuilder.Append("<div class=\"attachment-thumbnail govuk-!-margin-right-8\">");
            stringBuilder.Append("<a href=\"@Model.Uri\" download>");
            stringBuilder.Append(GetImageTag(fileExtension));
            stringBuilder.Append("</a></div>");
            stringBuilder.Append("<div class=\"attachment-details\">");
            stringBuilder.Append("<h2 class=\"attachment-title\">");
            stringBuilder.Append($"<a href=\"{uri}\" aria-describedby=\"file-details\" class=\"govuk-link attachment-link\" download>{title}");
            stringBuilder.Append("</a></h2>");
            stringBuilder.Append("<p class=\"attachment-metadata\" id=\"file-details\">");
            stringBuilder.Append($"<span class=\"attachment-attribute\" aria-label=\"file type\">{fileExtension.ToUpper()}</span>,\"");
            stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"file size\">");
            stringBuilder.Append($"{customAttachment.Size / 1024} KB");
            stringBuilder.Append("</span></p>");
            if (customAttachment.UpdatedAt.HasValue)
            {
                stringBuilder.Append("<p class=\"attachment-metadata\">");
                stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"updated date\">Last updated");
                stringBuilder.Append(customAttachment.UpdatedAt.Value.ToString("d MMMM yyyy"));
                stringBuilder.Append("</span></p>");
            }
            stringBuilder.Append("</div></div></div>");
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

    private CustomAttachment GenerateCustomAttachment(RichTextContentData target)
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

    private string GetImageTag(string fileExtension)
    {
        switch (fileExtension)
        {
            case "pdf":
                return "<img src=\"/assets/images/pdf-file-icon.svg\" alt=\"pdf file type\" >";
            case "csv":
            case "xls":
            case "xlsx":
                return "<img src=\"/assets/images/spreadsheet-file-icon.svg\" alt=\"spreadsheet file type\" />";
            case "html":
            case "htm":
                return "<img src =\"/assets/images/html-file-icon.svg\" alt=\"html file type\">";
            default:
                return "<img src =\"/assets/images/generic-file-icon.svg\" alt=\"generic file type\">";
        }
    }
}
