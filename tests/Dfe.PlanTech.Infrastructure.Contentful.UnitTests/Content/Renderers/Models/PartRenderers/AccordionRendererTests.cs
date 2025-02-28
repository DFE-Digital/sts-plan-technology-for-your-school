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
    public void CheckContentCorrectlyRenderedByAccordion()
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
}
