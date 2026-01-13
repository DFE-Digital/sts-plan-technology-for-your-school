using System.Text;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Rendering;

public class AccordionComponentRendererRendererTests
{
    private readonly ILogger<AccordionComponentRenderer> _logger = Substitute.For<ILogger<AccordionComponentRenderer>>();


    [Fact]
    public void CheckNoContentReturnsEmptyStringBuilder()
    {
        var renderer = new AccordionComponentRenderer(_logger);

        var stringBuilder = new StringBuilder();

        var content = new RichTextContentField()
        {
            NodeType = RichTextNodeType.EmbeddedEntryBlock.ToString(),
            Data = new RichTextContentSupportDataField()
        };

        var rendererCollection = new AccordionComponentRenderer(_logger);

        var result = renderer.AddHtml(content, rendererCollection, stringBuilder);

        Assert.Equal(result, stringBuilder);
    }

    [Fact]
    public void CheckSingleContentCorrectlyRenderedByAccordion()
    {
        var renderer = new AccordionComponentRenderer(_logger);

        var stringBuilder = new StringBuilder();

        var content = new RichTextContentField()
        {
            NodeType = RichTextNodeType.EmbeddedEntryBlock.ToString(),
            Data = new RichTextContentSupportDataField()
            {
                Target = new RichTextContentDataEntry()
                {
                    Content =
                    [
                        new RichTextContentDataEntry()
                        {
                            InternalName = "Internal Name 1",
                            Title = "Title 1",
                            SummaryLine = "Summary Line 1",
                            RichText = new RichTextContentField()
                            {
                                Description = "This is just a description and has no render content"
                            }
                        },
                    ]
                }
            }
        };

        var rendererCollection = new AccordionComponentRenderer(_logger);

        var result = renderer.AddHtml(content, rendererCollection, stringBuilder);

        Assert.Equal(GetStandardStringBuilderOutput().ToString(), result.ToString());
    }

    private StringBuilder GetStandardStringBuilderOutput()
    {
        var accId = "accordion-Internal Name 1";
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"<div class=\"govuk-accordion\" data-module=\"govuk-accordion\" id=\"{accId}\">");
        stringBuilder.Append("<div class=\"govuk-accordion__section\">");
        stringBuilder.Append("<div class=\"govuk-accordion__section-header\">");
        stringBuilder.Append("<h2 class=\"govuk-accordion__section-heading\">");
        stringBuilder.Append($"<span class=\"govuk-accordion__section-button\" id=\"{accId}-heading-1\">");
        stringBuilder.Append("Title 1");
        stringBuilder.Append("</span></h2>");
        stringBuilder.Append($"<div class=\"govuk-accordion__section-summary govuk-body\" id=\"{accId}-summary-1\">");
        stringBuilder.Append("Summary Line 1");
        stringBuilder.Append("</div></div>");
        stringBuilder.Append($"<div id=\"{accId}-content-1\" class=\"govuk-accordion__section-content\">");
        stringBuilder.Append("</div></div>");
        stringBuilder.Append("</div>");

        return stringBuilder;
    }
}
