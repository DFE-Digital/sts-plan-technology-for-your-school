using System.Text;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;

public class AttachmentComponent
{
    private string? _InternalName { get; set; }
    private string? _ContentType { get; set; }
    private string? _uri { get; set; }
    private string? _title { get; set; }
    private string? _fileExtension { get; set; }
    private long? _size { get; set; }
    private DateTime? _updatedAt { get; set; }

    public AttachmentComponent()
    {
    }

    public StringBuilder AddHtml(RichTextContent content, StringBuilder stringBuilder)
    {
        populate(content);

        stringBuilder.Append("<div class=\"guidance-container govuk-!-padding-8 govuk-!-margin-bottom-8 govuk-!-display-none-print\">");
        stringBuilder.Append("<div class=\"attachment\">");
        stringBuilder.Append("<div class=\"attachment-thumbnail govuk-!-margin-right-8\">");
        stringBuilder.Append("<a href=\"@Model.Uri\" download>");
        stringBuilder.Append(GetImageTag());
        stringBuilder.Append("</a></div>");
        stringBuilder.Append("<div class=\"attachment-details\">");
        stringBuilder.Append("<h2 class=\"attachment-title\">");
        stringBuilder.Append($"<a href=\"{_uri}\" aria-describedby=\"file-details\" class=\"govuk-link attachment-link\" download>{_title}");
        stringBuilder.Append("</a></h2>");

        stringBuilder.Append("<p class=\"attachment-metadata\" id=\"file-details\">");
        stringBuilder.Append($"<span class=\"attachment-attribute\" aria-label=\"file type\">{_fileExtension.ToUpper()}</span>,");
        stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"file size\">");
        stringBuilder.Append($"{_size / 1024} KB");
        stringBuilder.Append("</span></p>");

        if (_updatedAt.HasValue)
        {
            stringBuilder.Append("<p class=\"attachment-metadata\">");
            stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"updated date\">Last updated");
            stringBuilder.Append(_updatedAt.Value.ToString("d MMMM yyyy"));
            stringBuilder.Append("</span></p>");
        }
        stringBuilder.Append("</div></div></div>");

        return stringBuilder;
    }

    private void populate(RichTextContent content)
    {
        var target = content?.Data?.Target;
        _InternalName = target?.InternalName;
        _ContentType = target?.Asset.File.ContentType;
        _size = target?.Asset?.File?.Details?.Size;
        _title = target?.Title;
        _uri = target?.Asset.File.Url;
        _updatedAt = target?.Asset.SystemProperties.UpdatedAt;
        _fileExtension = _ContentType?.Split('/')[^1].ToLower();

        if (_fileExtension == "vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            _fileExtension = "xlsx";
        }
    }

    private string GetImageTag()
    {
        switch (_fileExtension)
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
