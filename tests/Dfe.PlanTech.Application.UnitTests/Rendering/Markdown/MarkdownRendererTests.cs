using Dfe.PlanTech.Application.Rendering.Markdown;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.UnitTests.Rendering.Markdown;

public class MarkdownRendererTests
{
    private static RichTextContentField Text(string value) =>
        new() { NodeType = "text", Value = value };

    private static RichTextContentField Paragraph(params RichTextContentField[] children) =>
        new() { NodeType = "paragraph", Content = [.. children] };

    private static RichTextContentField Heading(
        int level,
        params RichTextContentField[] children
    ) => new() { NodeType = $"heading-{level}", Content = [.. children] };

    private static RichTextContentField Document(params RichTextContentField[] children) =>
        new() { NodeType = "document", Content = [.. children] };

    private static RichTextContentField ListItem(params RichTextContentField[] children) =>
        new() { NodeType = "list-item", Content = [.. children] };

    private static RichTextContentField UL(params RichTextContentField[] children) =>
        new() { NodeType = "unordered-list", Content = [.. children] };

    private static RichTextContentField OL(params RichTextContentField[] children) =>
        new() { NodeType = "ordered-list", Content = [.. children] };

    private static RichTextContentField Quote(params RichTextContentField[] children) =>
        new() { NodeType = "blockquote", Content = [.. children] };

    private static RichTextContentField Link(string text, string uri) =>
        new()
        {
            NodeType = "hyperlink",
            Data = new() { Uri = uri },
            Content = [Text(text)],
        };

    private static string Render(RichTextContentField doc) => MarkdownRenderer.Render(doc);

    [Fact]
    public void Paragraph_renders_text()
    {
        var doc = Document(Paragraph(Text("Hello world")));

        var result = Render(doc);

        Assert.Equal("Hello world" + Environment.NewLine, result);
    }

    [Fact]
    public void Collapses_multiple_spaces_and_nbsp()
    {
        var nbsp = "\u00A0";

        var doc = Document(Paragraph(Text($"Hello{nbsp}{nbsp}   world")));

        var result = Render(doc);

        Assert.Equal("Hello world" + Environment.NewLine, result);
    }

    [Fact]
    public void Heading_level_1_uses_single_hash()
    {
        var doc = Document(Heading(1, Text("Title")));

        var result = Render(doc);

        Assert.Equal("# Title" + Environment.NewLine, result);
    }

    [Fact]
    public void Heading_level_3_maps_to_double_hash()
    {
        var doc = Document(Heading(3, Text("Sub")));

        var result = Render(doc);

        Assert.Equal("## Sub" + Environment.NewLine, result);
    }

    [Fact]
    public void Blockquote_prefixes_lines()
    {
        var doc = Document(Quote(Paragraph(Text("quoted text"))));

        var result = Render(doc);

        Assert.Equal("> quoted text" + Environment.NewLine, result);
    }

    [Fact]
    public void Unordered_list_renders_bullets()
    {
        var doc = Document(UL(ListItem(Paragraph(Text("A"))), ListItem(Paragraph(Text("B")))));

        var result = Render(doc);

        Assert.Equal("* A" + Environment.NewLine + "* B" + Environment.NewLine, result);
    }

    [Fact]
    public void Ordered_list_numbers_items()
    {
        var doc = Document(OL(ListItem(Paragraph(Text("A"))), ListItem(Paragraph(Text("B")))));

        var result = Render(doc);

        Assert.Equal("1. A" + Environment.NewLine + "2. B" + Environment.NewLine, result);
    }

    [Fact]
    public void Nested_list_indents()
    {
        var doc = Document(
            UL(ListItem(Paragraph(Text("Parent")), UL(ListItem(Paragraph(Text("Child"))))))
        );

        var result = Render(doc);

        Assert.Equal("* Parent" + Environment.NewLine + "  * Child" + Environment.NewLine, result);
    }

    [Fact]
    public void Hyperlink_renders_markdown_link()
    {
        var doc = Document(Paragraph(Link("Gov", "https://gov.uk")));

        var result = Render(doc);

        Assert.Equal("[Gov](https://gov.uk)" + Environment.NewLine, result);
    }

    [Fact]
    public void Relative_link_gets_absolute_prefix()
    {
        var doc = Document(Paragraph(Link("Page", "/abc")));

        var result = Render(doc);

        Assert.Equal(
            "[Page](https://plan-technology-for-your-school.education.gov.uk/abc)"
                + Environment.NewLine,
            result
        );
    }

    [Fact]
    public void Unknown_node_falls_back_to_children()
    {
        var weird = new RichTextContentField
        {
            NodeType = "mystery",
            Content = [Paragraph(Text("hello"))],
        };

        var doc = Document(weird);

        var result = Render(doc);

        Assert.Equal("hello" + Environment.NewLine, result);
    }

    [Fact]
    public void Ensures_single_trailing_newline_only()
    {
        var doc = Document(Paragraph(Text("A")), Paragraph(Text("B")));

        var result = Render(doc);

        Assert.Equal(
            "A" + Environment.NewLine + Environment.NewLine + "B" + Environment.NewLine,
            result
        );
    }
}
