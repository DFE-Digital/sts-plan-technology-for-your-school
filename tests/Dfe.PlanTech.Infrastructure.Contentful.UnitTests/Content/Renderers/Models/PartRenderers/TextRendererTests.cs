using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models.PartRenderers;

public class TextRendererTests
{
    private const string NODE_TYPE = "text";

    [Fact]
    public void Should_Accept_When_ContentIs_Text()
    {
        const string value = "Text value";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = value,
        };

        var renderer = new TextRenderer(new TextRendererOptions(new NullLogger<TextRendererOptions>(), new List<MarkOption>() { }));

        var accepted = renderer.Accepts(content);

        Assert.True(accepted);
    }

    [Fact]
    public void Should_Reject_When_Not_Text()
    {
        var content = new RichTextContent()
        {
            NodeType = "hyperlink",
            Value = "hyperlink"
        };

        var renderer = new TextRenderer(new TextRendererOptions(new NullLogger<TextRendererOptions>(), new List<MarkOption>() { }));

        var accepted = renderer.Accepts(content);

        Assert.False(accepted);
    }

    [Fact]
    public void Should_Create_Strong_When_Has_BoldMark()
    {
        const string boldType = "bold";
        const string htmlTagForBold = "strong";

        var boldMarkOption = new MarkOption()
        {
            Mark = boldType,
            HtmlTag = htmlTagForBold,
        };

        var renderer = new TextRenderer(new TextRendererOptions(new NullLogger<TextRendererOptions>(), new List<MarkOption>() { boldMarkOption }));
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string value = "Paragraph text";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = value,
            Marks = new() {
                new RichTextMark() {
                    Type = boldType,
                }
            }
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Equal($"<{htmlTagForBold}>{value}</{htmlTagForBold}>", html);
    }

    [Fact]
    public void Should_AddClasses_When_MarkOptionHasClasses()
    {
        const string boldType = "bold";
        const string htmlTagForBold = "strong";
        const string testClasses = "testing-classes";

        var boldMarkOption = new MarkOption()
        {
            Mark = boldType,
            HtmlTag = htmlTagForBold,
            Classes = testClasses
        };

        var renderer = new TextRenderer(new TextRendererOptions(new NullLogger<TextRendererOptions>(), new List<MarkOption>() { boldMarkOption }));
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string value = "Paragraph text";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = value,
            Marks = new() {
                new RichTextMark() {
                    Type = boldType,
                }
            }
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Equal($"<{htmlTagForBold} class=\"{testClasses}\">{value}</{htmlTagForBold}>", html);
    }


    [Fact]
    public void Should_RenderText_When_HasNoMarks()
    {
        const string boldType = "bold";
        const string htmlTagForBold = "strong";
        const string testClasses = "testing-classes";

        var boldMarkOption = new MarkOption()
        {
            Mark = boldType,
            HtmlTag = htmlTagForBold,
            Classes = testClasses
        };

        var renderer = new TextRenderer(new TextRendererOptions(new NullLogger<TextRendererOptions>(), new List<MarkOption>() { boldMarkOption }));
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string value = "Paragraph text";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = value,
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Equal(value, html);
    }
}
