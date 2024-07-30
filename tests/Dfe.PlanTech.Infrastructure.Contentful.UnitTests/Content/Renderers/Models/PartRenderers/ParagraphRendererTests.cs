using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models.PartRenderers;

public class ParagraphRendererTests
{
    private const string NODE_TYPE = "paragraph";

    [Fact]
    public void Should_Accept_When_ContentIs_Paragraph()
    {
        const string value = "Paragraph value";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = value,
        };

        var renderer = new ParagraphRenderer(new ParagraphRendererOptions());

        var accepted = renderer.Accepts(content);

        Assert.True(accepted);
    }

    [Fact]
    public void Should_Reject_When_Not_Paragraph()
    {
        var content = new RichTextContent()
        {
            NodeType = "hyperlink",
            Value = "hyperlink"
        };

        var renderer = new ParagraphRenderer(new ParagraphRendererOptions());

        var accepted = renderer.Accepts(content);

        Assert.False(accepted);
    }

    [Fact]
    public void Should_Create_Paragraph_When_PassedValidData()
    {
        var renderer = new ParagraphRenderer(new ParagraphRendererOptions());
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string value = "Paragraph text";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = value,
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Equal("<p></p>", html);
    }


    [Fact]
    public void Should_Create_Paragraph_WithClass__When_PassedValidData()
    {
        const string classes = "testing-classes";
        var renderer = new ParagraphRenderer(new ParagraphRendererOptions() { Classes = classes });
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string value = "Paragraph text";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = value,
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Equal($"<p class=\"{classes}\"></p>", html);
    }
}
