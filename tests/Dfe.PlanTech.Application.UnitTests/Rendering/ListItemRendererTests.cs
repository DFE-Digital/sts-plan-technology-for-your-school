using System.Text;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.PlanTech.Application.UnitTests.Rendering;

public class ListItemRendererTests
{
    private const string NODE_TYPE = "list-item";

    [Fact]
    public void Should_Accept_When_ContentIsListItem()
    {
        const string listItemValue = "List item one";

        var content = new RichTextContentField()
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
        var content = new RichTextContentField()
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

        var content = new RichTextContentField()
        {
            NodeType = NODE_TYPE,
            Value = listItemValue,
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Equal("<li></li>", html);
    }
}
