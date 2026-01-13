using System.Text;
using Dfe.PlanTech.Application.Rendering.Models;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.Rendering;

public static class AttachmentComponentRenderer
{
    public static StringBuilder AddHtml(RichTextContentField content, StringBuilder stringBuilder)
    {
        var target = content?.Data?.Target;

        if (target == null)
        {
            return stringBuilder;
        }

        var customAttachment = new CustomAttachmentModel(target);

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

    private static string GetImageTag(string fileExtension)
    {
        const string pdf = "<img src=\"/assets/images/pdf-file-icon.svg\" alt=\"pdf file type\" >";
        const string table = "<img src=\"/assets/images/spreadsheet-file-icon.svg\" alt=\"spreadsheet file type\" />";
        const string htm = "<img src =\"/assets/images/html-file-icon.svg\" alt=\"html file type\">";
        const string @default = "<img src =\"/assets/images/generic-file-icon.svg\" alt=\"generic file type\">";

        return fileExtension switch
        {
            FileExtensionConstants.PDF => pdf,
            FileExtensionConstants.CSV or FileExtensionConstants.XLS or FileExtensionConstants.XLSX => table,
            FileExtensionConstants.HTML or FileExtensionConstants.HTM => htm,
            _ => @default
        };
    }
}
