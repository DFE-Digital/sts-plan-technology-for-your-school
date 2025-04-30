using System.Text;
using Dfe.PlanTech.Domain.Constants;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class AttachmentComponent
{
    public AttachmentComponent()
    {
    }

    public StringBuilder AddHtml(RichTextContent content, StringBuilder stringBuilder)
    {
        var target = content?.Data?.Target;

        if (target == null)
        {
            return stringBuilder;
        }

        var customAttachment = GenerateCustomAttachment(target);

        stringBuilder.Append("<div class=\"guidance-container govuk-!-padding-8 govuk-!-margin-bottom-8 govuk-!-display-none-print govuk-body \">");
        stringBuilder.Append("<div class=\"attachment\">");
        stringBuilder.Append("<div class=\"attachment-thumbnail govuk-!-margin-right-8\">");
        stringBuilder.Append($"<a href=\"{customAttachment.Uri}\" download>");
        stringBuilder.Append(GetImageTag(customAttachment.FileExtension!));
        stringBuilder.Append("</a></div>");
        stringBuilder.Append("<div class=\"attachment-details\">");
        stringBuilder.Append("<h2 class=\"attachment-title\">");
        stringBuilder.Append($"<a href=\"{customAttachment.Uri}\" aria-describedby=\"file-details\" class=\"govuk-link attachment-link\" download>{customAttachment.Title}");
        stringBuilder.Append("</a></h2>");

        stringBuilder.Append("<p class=\"attachment-metadata\" id=\"file-details\">");
        stringBuilder.Append($"<span class=\"attachment-attribute\" aria-label=\"file type\">{customAttachment.FileExtension!.ToUpper()}</span>,");
        stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"file size\">");
        stringBuilder.Append($"{customAttachment.Size} KB");
        stringBuilder.Append("</span></p>");

        if (customAttachment.UpdatedAt.HasValue)
        {
            stringBuilder.Append("<p class=\"attachment-metadata\">");
            stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"updated date\">Last updated ");
            stringBuilder.Append(customAttachment.UpdatedAt.Value.ToString("d MMMM yyyy"));
            stringBuilder.Append("</span></p>");
        }
        stringBuilder.Append("</div></div></div>");

        return stringBuilder;
    }

    private CustomAttachment GenerateCustomAttachment(RichTextContentData content)
    {
        var contentType = content?.Asset.File.ContentType;
        var fileExtension = contentType?.Split('/')[^1].ToLower();

        if (fileExtension == FileExtensions.XLSXSPREADSHEET)
        {
            fileExtension = FileExtensions.XLSX;
        }

        return new CustomAttachment
        {
            InternalName = content?.InternalName ?? string.Empty,
            ContentType = contentType ?? string.Empty,
            Size = content?.Asset?.File?.Details?.Size / 1024 ?? 0,
            Title = content?.Title,
            Uri = content?.Asset.File.Url ?? string.Empty,
            UpdatedAt = content?.Asset.SystemProperties.UpdatedAt,
            FileExtension = fileExtension ?? string.Empty,
        };
    }

    private string GetImageTag(string fileExtension)
    {
        switch (fileExtension)
        {
            case FileExtensions.PDF:
                return "<img src=\"/assets/images/pdf-file-icon.svg\" alt=\"pdf file type\" >";
            case FileExtensions.CSV:
            case FileExtensions.XLS:
            case FileExtensions.XLSX:
                return "<img src=\"/assets/images/spreadsheet-file-icon.svg\" alt=\"spreadsheet file type\" />";
            case FileExtensions.HTML:
            case FileExtensions.HTM:
                return "<img src =\"/assets/images/html-file-icon.svg\" alt=\"html file type\">";
            default:
                return "<img src =\"/assets/images/generic-file-icon.svg\" alt=\"generic file type\">";
        }
    }
}
