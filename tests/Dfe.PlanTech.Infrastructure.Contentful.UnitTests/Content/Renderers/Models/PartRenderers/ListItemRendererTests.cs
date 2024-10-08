using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models.PartRenderers;

public class ListItemRendererTests
{
    private const string NODE_TYPE = "list-item";

    [Fact]
    public void Should_Accept_When_ContentIsListItem()
    {
        const string listItemValue = "List item one";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = listItemValue,
        };

        var renderer = new ListItemRenderer();

        var accepted = renderer.Accepts(content);

        Assert.True(accepted);
    }

    [Fact]
    public void Should_Reject_When_Not_ListItem()
    {
        var content = new RichTextContent()
        {
            NodeType = "paragraph",
            Value = "paragraph text"
        };

        var renderer = new ListItemRenderer();

        var accepted = renderer.Accepts(content);

        Assert.False(accepted);
    }

    [Fact]
    public void Should_CreateListItem_When_PassedValidData()
    {
        var renderer = new ListItemRenderer();
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string listItemValue = "List item one";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = listItemValue,
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Equal("<li></li>", html);
    }
}
