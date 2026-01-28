using System.Text;
using Contentful.Core.Models;
using Dfe.PlanTech.Data.Contentful.Content.Renderers.Models.PartRenderers;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using ContentfulFile = Contentful.Core.Models.File;

namespace Dfe.PlanTech.Data.Contentful.UnitTests.Content.Renderers.Models.PartRenderers;

public class AttachmentRendererTests
{
    [Fact]
    public void CheckEmptyContentReturnsNewStringBuilder()
    {
        var renderer = new AttachmentComponent();
        var result = renderer.AddHtml(new RichTextContent(), new StringBuilder());

        Assert.Equal(new StringBuilder().ToString(), result.ToString());
    }

    [Fact]
    public void CheckPdfGeneratedCorrectly()
    {
        var renderer = new AttachmentComponent();
        var content = GetContent();
        content.Data!.Target!.Asset.File.ContentType = "pdf";

        var result = renderer.AddHtml(content, new StringBuilder());

        Assert.Equal(GetPdfStringBuilderOutput().ToString(), result.ToString());
    }

    [Fact]
    public void CheckHtmlGeneratedCorrectly()
    {
        var renderer = new AttachmentComponent();
        var content = GetContent();
        content.Data!.Target!.Asset.File.ContentType = "html";

        var result = renderer.AddHtml(content, new StringBuilder());

        Assert.Equal(GetHtmlStringBuilderOutput().ToString(), result.ToString());
    }

    [Fact]
    public void CheckAttachmentsGeneratedCorrectly()
    {
        var renderer = new AttachmentComponent();
        var result = renderer.AddHtml(GetContent(), new StringBuilder());

        Assert.Equal(GetStandardStringBuilderOutput().ToString(), result.ToString());
    }

    private RichTextContent GetContent()
    {
        return new RichTextContent()
        {
            NodeType = RichTextNodeType.EmbeddedEntryBlock.ToString(),
            Data = new RichTextContentSupportData()
            {
                Target = new RichTextContentData()
                {
                    Title = "The Title",
                    InternalName = "TestSlug",
                    Asset = new Asset()
                    {
                        File = new ContentfulFile
                        {
                            ContentType = "vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            Url = "test.com",
                            Details = new FileDetails() { Size = 2048 },
                        },
                        SystemProperties = new SystemProperties()
                        {
                            UpdatedAt = DateTime.Parse("2025-01-01"),
                        },
                    },
                },
            },
        };
    }

    private StringBuilder GetStandardStringBuilderOutput()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(
            "<div class=\"guidance-container govuk-!-padding-8 govuk-!-margin-bottom-8 govuk-!-display-none-print govuk-body \">"
        );
        stringBuilder.Append("<div class=\"attachment\">");
        stringBuilder.Append("<div class=\"attachment-thumbnail govuk-!-margin-right-8\">");
        stringBuilder.Append("<a href=\"test.com\" download>");
        stringBuilder.Append(
            "<img src=\"/assets/images/spreadsheet-file-icon.svg\" alt=\"spreadsheet file type\" />"
        );
        stringBuilder.Append("</a></div>");
        stringBuilder.Append("<div class=\"attachment-details\">");
        stringBuilder.Append("<h2 class=\"attachment-title\">");
        stringBuilder.Append(
            "<a href=\"test.com\" aria-describedby=\"file-details\" class=\"govuk-link attachment-link\" download>The Title"
        );
        stringBuilder.Append("</a></h2>");

        stringBuilder.Append("<p class=\"attachment-metadata\" id=\"file-details\">");
        stringBuilder.Append(
            "<span class=\"attachment-attribute\" aria-label=\"file type\">XLSX</span>,"
        );
        stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"file size\">");
        stringBuilder.Append("2 KB");
        stringBuilder.Append("</span></p>");

        stringBuilder.Append("<p class=\"attachment-metadata\">");
        stringBuilder.Append(
            "<span class=\"attachment-attribute\" aria-label=\"updated date\">Last updated 1 January 2025"
        );
        stringBuilder.Append("</span></p>");

        stringBuilder.Append("</div></div></div>");

        return stringBuilder;
    }

    private StringBuilder GetPdfStringBuilderOutput()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(
            "<div class=\"guidance-container govuk-!-padding-8 govuk-!-margin-bottom-8 govuk-!-display-none-print govuk-body \">"
        );
        stringBuilder.Append("<div class=\"attachment\">");
        stringBuilder.Append("<div class=\"attachment-thumbnail govuk-!-margin-right-8\">");
        stringBuilder.Append("<a href=\"test.com\" download>");
        stringBuilder.Append(
            "<img src=\"/assets/images/pdf-file-icon.svg\" alt=\"pdf file type\" >"
        );
        stringBuilder.Append("</a></div>");
        stringBuilder.Append("<div class=\"attachment-details\">");
        stringBuilder.Append("<h2 class=\"attachment-title\">");
        stringBuilder.Append(
            "<a href=\"test.com\" aria-describedby=\"file-details\" class=\"govuk-link attachment-link\" download>The Title"
        );
        stringBuilder.Append("</a></h2>");

        stringBuilder.Append("<p class=\"attachment-metadata\" id=\"file-details\">");
        stringBuilder.Append(
            "<span class=\"attachment-attribute\" aria-label=\"file type\">PDF</span>,"
        );
        stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"file size\">");
        stringBuilder.Append("2 KB");
        stringBuilder.Append("</span></p>");

        stringBuilder.Append("<p class=\"attachment-metadata\">");
        stringBuilder.Append(
            "<span class=\"attachment-attribute\" aria-label=\"updated date\">Last updated 1 January 2025"
        );
        stringBuilder.Append("</span></p>");

        stringBuilder.Append("</div></div></div>");

        return stringBuilder;
    }

    private StringBuilder GetHtmlStringBuilderOutput()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(
            "<div class=\"guidance-container govuk-!-padding-8 govuk-!-margin-bottom-8 govuk-!-display-none-print govuk-body \">"
        );
        stringBuilder.Append("<div class=\"attachment\">");
        stringBuilder.Append("<div class=\"attachment-thumbnail govuk-!-margin-right-8\">");
        stringBuilder.Append("<a href=\"test.com\" download>");
        stringBuilder.Append(
            "<img src =\"/assets/images/html-file-icon.svg\" alt=\"html file type\">"
        );
        stringBuilder.Append("</a></div>");
        stringBuilder.Append("<div class=\"attachment-details\">");
        stringBuilder.Append("<h2 class=\"attachment-title\">");
        stringBuilder.Append(
            "<a href=\"test.com\" aria-describedby=\"file-details\" class=\"govuk-link attachment-link\" download>The Title"
        );
        stringBuilder.Append("</a></h2>");

        stringBuilder.Append("<p class=\"attachment-metadata\" id=\"file-details\">");
        stringBuilder.Append(
            "<span class=\"attachment-attribute\" aria-label=\"file type\">HTML</span>,"
        );
        stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"file size\">");
        stringBuilder.Append("2 KB");
        stringBuilder.Append("</span></p>");

        stringBuilder.Append("<p class=\"attachment-metadata\">");
        stringBuilder.Append(
            "<span class=\"attachment-attribute\" aria-label=\"updated date\">Last updated 1 January 2025"
        );
        stringBuilder.Append("</span></p>");

        stringBuilder.Append("</div></div></div>");

        return stringBuilder;
    }
}
