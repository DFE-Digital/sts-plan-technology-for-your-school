using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models.PartRenderers;

public class HeadingRendererTests
{
    [Theory]
    [InlineData("heading-1")]
    [InlineData("heading-2")]
    [InlineData("heading-3")]
    [InlineData("heading-4")]
    [InlineData("heading-5")]
    [InlineData("heading-6")]
    public void Should_Accept_When_ContentIsHeading(string nodeType)
    {
        var content = new RichTextContent()
        {
            NodeType = nodeType,
            Value = "",
            Content = []
        };

        var renderer = new HeadingRenderer();
        var accepted = renderer.Accepts(content);
        Assert.True(accepted);
    }

    [Fact]
    public void Should_Reject_When_Not_Heading()
    {
        var content = new RichTextContent()
        {
            NodeType = "paragraph",
            Value = "paragraph text"
        };

        var renderer = new HeadingRenderer();
        var accepted = renderer.Accepts(content);

        Assert.False(accepted);
    }

    [Theory]
    [InlineData("heading-1", "<h1></h1>")]
    [InlineData("heading-2", "<h2></h2>")]
    [InlineData("heading-3", "<h3></h3>")]
    [InlineData("heading-4", "<h4></h4>")]
    [InlineData("heading-5", "<h5></h5>")]
    [InlineData("heading-6", "<h6></h6>")]
    public void Should_Generate_Correct_Header_Tags(string nodeType, string expected)
    {
        var renderer = new HeadingRenderer();
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });
        var content = new RichTextContent()
        {
            NodeType = nodeType,
            Content = []
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());
        var html = result.ToString();

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData("heading-1", "<h1>test</h1>")]
    [InlineData("heading-2", "<h2>test</h2>")]
    [InlineData("heading-3", "<h3>test</h3>")]
    [InlineData("heading-4", "<h4>test</h4>")]
    [InlineData("heading-5", "<h5>test</h5>")]
    [InlineData("heading-6", "<h6>test</h6>")]
    public void Should_Generate_Correct_Header_Tags_With_Text_Rendering_Within(string nodeType, string expected)
    {
        var headingRenderer = new HeadingRenderer();
        var textRenderer = new TextRenderer(new TextRendererOptions(new NullLogger<TextRendererOptions>(), []));

        var rendererCollection = new RichTextRenderer(
            new NullLogger<RichTextRenderer>(),
            new List<BaseRichTextContentPartRender> { headingRenderer, textRenderer }
        );

        var content = new RichTextContent()
        {
            NodeType = nodeType,
            Content = [
                new RichTextContent()
                {
                    NodeType = "text",
                    Value = "test"
                }
            ]
        };

        var result = headingRenderer.AddHtml(content, rendererCollection, new StringBuilder());
        var html = result.ToString();

        Assert.Equal(expected, html);
    }
}
