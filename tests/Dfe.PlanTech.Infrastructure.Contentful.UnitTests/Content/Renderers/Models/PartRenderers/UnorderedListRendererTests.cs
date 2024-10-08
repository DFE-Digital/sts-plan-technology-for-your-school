using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models.PartRenderers;

public class UnorderedListRendererTests
{
    private const string NODE_TYPE = "unordered-list";

    [Fact]
    public void Should_Accept_When_ContentIs_UnorderedList()
    {
        const string value = "List item one";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = value,
        };

        var renderer = new UnorderedListRenderer();

        var accepted = renderer.Accepts(content);

        Assert.True(accepted);
    }

    [Fact]
    public void Should_Reject_When_Not_UnorderedList()
    {
        var content = new RichTextContent()
        {
            NodeType = "paragraph",
            Value = "paragraph text"
        };

        var renderer = new UnorderedListRenderer();

        var accepted = renderer.Accepts(content);

        Assert.False(accepted);
    }

    [Fact]
    public void Should_CreateUnorderedList_When_PassedValidData()
    {
        var renderer = new UnorderedListRenderer();
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string value = "List item one";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = value,
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Equal("<ul></ul>", html);
    }
}
