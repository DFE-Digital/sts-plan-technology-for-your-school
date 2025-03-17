using System.Text;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models.PartRenderers;

public class AccordionRendererTests
{
    private readonly ILogger<AccordionComponent> _logger = Substitute.For<ILogger<AccordionComponent>>();
    

    [Fact]
    public void CheckNoContentReturnsEmptyStringBuilder()
    {
        var renderer = new AccordionComponent(_logger);

        var stringBuilder = new StringBuilder();

        var content = new RichTextContent()
        {
            NodeType = RichTextNodeType.EmbeddedEntryBlock.ToString(),
            Data = new RichTextContentSupportData()
        };

        var rendererCollection = new AccordionComponent(_logger);

        var result = renderer.AddHtml(content, rendererCollection, stringBuilder);

        Assert.Equal(result, stringBuilder);
    }

    [Fact]
    public void CheckSingleContentCorrectlyRenderedByAccordion()
    {
        var renderer = new AccordionComponent(_logger);

        var stringBuilder = new StringBuilder();

        var content = new RichTextContent()
        {
            NodeType = RichTextNodeType.EmbeddedEntryBlock.ToString(),
            Data = new RichTextContentSupportData()
            {
                Target = new RichTextContentData()
                {
                    Content =
                    [
                        new RichTextContentData()
                        {
                            InternalName = "Internal Name 1",
                            Title = "Title 1",
                            SummaryLine = "Summary Line 1",
                            RichText = new RichTextContent()
                            {
                                Description = "This is just a description and has no render content"
                            }
                        },
                    ]
                }
            }
        };

        var rendererCollection = new AccordionComponent(_logger);

        var result = renderer.AddHtml(content, rendererCollection, stringBuilder);

        Assert.Equal(result.ToString(), GetStandardStringBuilderOutput().ToString());
    }

    private StringBuilder GetStandardStringBuilderOutput()
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
        stringBuilder.Append("</div>");
        return stringBuilder;
    }
}
