using System.Text;
using Contentful.Core.Models;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ContentfulFile = Contentful.Core.Models.File;

namespace Dfe.PlanTech.Application.UnitTests.Rendering;

public class EmbeddedEntryBlockRendererTests
{
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly ILogger<AccordionComponentRenderer> _accordionLogger = Substitute.For<ILogger<AccordionComponentRenderer>>();
    private readonly IRichTextContentPartRendererCollection _richTextContentPartRendererCollection = Substitute.For<IRichTextContentPartRendererCollection>();

    [Fact]
    public void ShouldReturnNull_WhenNoRichTextContent()
    {
        var renderer = new EmbeddedEntryBlockRenderer(_loggerFactory);

        var stringBuilder = new StringBuilder();

        var result = renderer.AddHtml(new RichTextContentField(), new AccordionComponentRenderer(_accordionLogger), stringBuilder);

        Assert.Equal(stringBuilder.ToString(), result.ToString());
    }

    [Fact]
    public void ShouldReturnData_WhenAttachmentSystemPropertyId()
    {
        var renderer = new EmbeddedEntryBlockRenderer(_loggerFactory);

        var result = renderer.AddHtml(GetGenericAttachmentContent(), _richTextContentPartRendererCollection, new StringBuilder());

        Assert.Equal(GetGenericAttachmentStringBuilderOutput().ToString(), result.ToString());
    }

    [Fact]
    public void ShouldReturnData_WhenAccordionSystemPropertyId()
    {
        var renderer = new EmbeddedEntryBlockRenderer(_loggerFactory);

        var result = renderer.AddHtml(GetCsAccordionContent(), new AccordionComponentRenderer(_accordionLogger), new StringBuilder());

        Assert.Equal(GetAccordionStringBuilderOutput().ToString(), result.ToString());
    }

    private RichTextContentField GetGenericAttachmentContent()
    {
        return new RichTextContentField()
        {
            NodeType = RichTextNodeType.EmbeddedEntryBlock.ToString(),
            Data = new RichTextContentSupportDataField()
            {
                Target = new RichTextContentDataEntry()
                {
                    Title = "The Title",
                    InternalName = "TestSlug",
                    Asset = new Asset()
                    {
                        File = new ContentfulFile
                        {
                            ContentType = "Wav",
                            Url = "test.com",
                            Details = new FileDetails()
                            {
                                Size = 2048
                            }
                        },
                        SystemProperties = new SystemProperties()
                        {
                            UpdatedAt = DateTime.Parse("2025-01-01"),
                        }
                    },
                    SystemProperties = new SystemProperties()
                    {
                        ContentType = new ContentType()
                        {
                            SystemProperties = new SystemProperties()
                            {
                                Id = ContentfulContentTypeConstants.ComponentAttachmentContentfulContentTypeId,
                            }
                        }
                    },
                }
            }
        };
    }

    private StringBuilder GetGenericAttachmentStringBuilderOutput()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append("<div class=\"guidance-container govuk-!-padding-8 govuk-!-margin-bottom-8 govuk-!-display-none-print govuk-body \">");
        stringBuilder.Append("<div class=\"attachment\">");
        stringBuilder.Append("<div class=\"attachment-thumbnail govuk-!-margin-right-8\">");
        stringBuilder.Append($"<a href=\"test.com\" download>");
        stringBuilder.Append("<img src =\"/assets/images/generic-file-icon.svg\" alt=\"generic file type\">");
        stringBuilder.Append("</a></div>");
        stringBuilder.Append("<div class=\"attachment-details\">");
        stringBuilder.Append("<h2 class=\"attachment-title\">");
        stringBuilder.Append("<a href=\"test.com\" aria-describedby=\"file-details\" class=\"govuk-link attachment-link\" download>The Title");
        stringBuilder.Append("</a></h2>");

        stringBuilder.Append("<p class=\"attachment-metadata\" id=\"file-details\">");
        stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"file type\">WAV</span>,");
        stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"file size\">");
        stringBuilder.Append("2 KB");
        stringBuilder.Append("</span></p>");

        stringBuilder.Append("<p class=\"attachment-metadata\">");
        stringBuilder.Append("<span class=\"attachment-attribute\" aria-label=\"updated date\">Last updated 1 January 2025");
        stringBuilder.Append("</span></p>");

        stringBuilder.Append("</div></div></div>");

        return stringBuilder;
    }

    private RichTextContentField GetCsAccordionContent()
    {
        return new RichTextContentField()
        {
            NodeType = RichTextNodeType.EmbeddedEntryBlock.ToString(),
            Data = new RichTextContentSupportDataField()
            {
                Target = new RichTextContentDataEntry()
                {
                    Title = "The Title",
                    InternalName = "TestSlug",
                    Asset = new Asset(),
                    SystemProperties = new SystemProperties()
                    {
                        ContentType = new ContentType()
                        {
                            SystemProperties = new SystemProperties()
                            {
                                Id = ContentfulContentTypeConstants.ComponentAccordionContentfulContentTypeId,
                            }
                        }
                    },
                    Content =
                    [
                        new()
                        {
                            InternalName = "Internal Name 1",
                            Title = "Title 1",
                            SummaryLine = "Summary Line 1",
                            RichText = new RichTextContentField(),
                        },
                        new()
                        {
                            InternalName = "Internal Name 2",
                            Title = "Title 2",
                            SummaryLine = "Summary Line 2",
                            RichText = new RichTextContentField(),
                        },
                        new()
                        {
                            InternalName = "Internal Name 3",
                            Title = "Title 3",
                            SummaryLine = "Summary Line 3",
                            RichText = new RichTextContentField(),
                        },
                    ]
                }
            }
        };
    }

    private StringBuilder GetAccordionStringBuilderOutput()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"<div class=\"govuk-accordion\" data-module=\"govuk-accordion\" id=\"accordion-Internal Name 1\">");
        stringBuilder.Append("<div class=\"govuk-accordion__section govuk-body\">");
        stringBuilder.Append("<div class=\"govuk-accordion__section-header\">");
        stringBuilder.Append("<h2 class=\"govuk-accordion__section-heading\">");
        stringBuilder.Append("<span class=\"govuk-accordion__section-button\" id=\"Internal Name 1-heading\">");
        stringBuilder.Append("Title 1");
        stringBuilder.Append("</span></h2>");
        stringBuilder.Append("<div class=\"govuk-accordion__section-summary govuk-body\" id=\"Internal Name 1-summary\">");
        stringBuilder.Append("Summary Line 1");
        stringBuilder.Append("</div></div>");
        stringBuilder.Append("<div id=\"Internal Name 1-content\" class=\"govuk-accordion__section-content\">");
        stringBuilder.Append("</div></div>");

        stringBuilder.Append("<div class=\"govuk-accordion__section govuk-body\">");
        stringBuilder.Append("<div class=\"govuk-accordion__section-header\">");
        stringBuilder.Append("<h2 class=\"govuk-accordion__section-heading\">");
        stringBuilder.Append("<span class=\"govuk-accordion__section-button\" id=\"Internal Name 2-heading\">");
        stringBuilder.Append("Title 2");
        stringBuilder.Append("</span></h2>");
        stringBuilder.Append("<div class=\"govuk-accordion__section-summary govuk-body\" id=\"Internal Name 2-summary\">");
        stringBuilder.Append("Summary Line 2");
        stringBuilder.Append("</div></div>");
        stringBuilder.Append("<div id=\"Internal Name 2-content\" class=\"govuk-accordion__section-content\">");
        stringBuilder.Append("</div></div>");

        stringBuilder.Append("<div class=\"govuk-accordion__section govuk-body\">");
        stringBuilder.Append("<div class=\"govuk-accordion__section-header\">");
        stringBuilder.Append("<h2 class=\"govuk-accordion__section-heading\">");
        stringBuilder.Append("<span class=\"govuk-accordion__section-button\" id=\"Internal Name 3-heading\">");
        stringBuilder.Append("Title 3");
        stringBuilder.Append("</span></h2>");
        stringBuilder.Append("<div class=\"govuk-accordion__section-summary govuk-body\" id=\"Internal Name 3-summary\">");
        stringBuilder.Append("Summary Line 3");
        stringBuilder.Append("</div></div>");
        stringBuilder.Append("<div id=\"Internal Name 3-content\" class=\"govuk-accordion__section-content\">");
        stringBuilder.Append("</div></div>");
        stringBuilder.Append("</div>");
        return stringBuilder;
    }

}
