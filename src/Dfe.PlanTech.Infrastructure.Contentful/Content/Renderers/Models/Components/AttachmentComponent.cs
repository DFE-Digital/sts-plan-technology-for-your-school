using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped.Custom;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.Components;

public class AttachmentComponent
{
    public AttachmentComponent()
    {
    }
    public StringBuilder AddHtml(RichTextContent content, StringBuilder stringBuilder)
    {
        var target = content?.Data?.Target ?? null;
        var customComponent = GenerateCustomAttachment(target);
        var uri = customComponent.Uri;
        var title = customComponent.Title;
        var fileExtension = customComponent.ContentType.Split('/')[^1].ToLower();
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
        stringBuilder.Append($"<span class=\"attachment-attribute\" aria-label=\"file type\">{fileExtension.ToUpper()}</span>,");
        stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"file size\">");
        stringBuilder.Append($"{customComponent.Size / 1024} KB");
        stringBuilder.Append("</span></p>");
        if (customComponent.UpdatedAt.HasValue)
        {
            stringBuilder.Append("<p class=\"attachment-metadata\">");
            stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"updated date\">Last updated");
            stringBuilder.Append(customComponent.UpdatedAt.Value.ToString("d MMMM yyyy"));
            stringBuilder.Append("</span></p>");
        }
        stringBuilder.Append("</div></div></div>");

        return stringBuilder;
    }

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
