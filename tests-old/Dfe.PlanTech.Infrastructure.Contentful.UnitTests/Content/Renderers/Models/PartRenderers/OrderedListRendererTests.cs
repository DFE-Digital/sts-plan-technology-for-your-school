using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Data.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.PlanTech.Data.Contentful.UnitTests.Content.Renderers.Models.PartRenderers;

public class OrderedListRendererTests
{
    private const string NODE_TYPE = "ordered-list";

    [Fact]
    public void Should_Accept_When_ContentIs_OrderedList()
    {
        const string listItemValue = "List item one";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = listItemValue,
        };

        var renderer = new OrderedListRenderer();

        var accepted = renderer.Accepts(content);

        Assert.True(accepted);
    }

    [Fact]
    public void Should_Reject_When_NotHyperlink()
    {
        var content = new RichTextContent()
        {
            NodeType = "paragraph",
            Value = "paragraph text"
        };

        var renderer = new OrderedListRenderer();

        var accepted = renderer.Accepts(content);

        Assert.False(accepted);
    }

    [Fact]
    public void Should_CreateOrderedList_When_PassedValidData()
    {
        var renderer = new OrderedListRenderer();
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string listItemValue = "List item one";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = listItemValue,
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Equal("<ol></ol>", html);
    }
}
