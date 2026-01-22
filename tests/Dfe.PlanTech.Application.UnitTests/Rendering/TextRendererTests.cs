using System.Text;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Contentful.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Rendering;

public class TextRendererTests
{
    private readonly ILogger<TextRendererOptions> _logger = Substitute.For<
        ILogger<TextRendererOptions>
    >();

    private const string NODE_TYPE = "text";

    private Application.Rendering.TextRenderer BuildSut(List<MarkOption>? markOptions = null)
    {
        return new Application.Rendering.TextRenderer(
            new TextRendererOptions(_logger, markOptions ?? [])
        );
    }

    [Fact]
    public void Should_Accept_When_ContentIs_Text()
    {
        const string value = "Text value";

        var content = new RichTextContentField() { NodeType = NODE_TYPE, Value = value };

        var renderer = BuildSut();

        var accepted = renderer.Accepts(content);

        Assert.True(accepted);
    }

    [Fact]
    public void Should_Reject_When_Not_Text()
    {
        var content = new RichTextContentField() { NodeType = "hyperlink", Value = "hyperlink" };

        var renderer = BuildSut();

        var accepted = renderer.Accepts(content);

        Assert.False(accepted);
    }

    [Fact]
    public void Should_Create_Strong_When_Has_BoldMark()
    {
        const string boldType = "bold";
        const string htmlTagForBold = "strong";

        var boldMarkOption = new MarkOption() { Mark = boldType, HtmlTag = htmlTagForBold };

        var renderer = BuildSut([boldMarkOption]);
        var rendererCollection = new RichTextRenderer(
            new NullLogger<RichTextRenderer>(),
            [renderer]
        );

        const string value = "Paragraph text";

        var content = new RichTextContentField()
        {
            NodeType = NODE_TYPE,
            Value = value,
            Marks = new() { new RichTextMarkField() { Type = boldType } },
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
            Classes = testClasses,
        };

        var renderer = BuildSut([boldMarkOption]);
        var rendererCollection = new RichTextRenderer(
            new NullLogger<RichTextRenderer>(),
            [renderer]
        );

        const string value = "Paragraph text";

        var content = new RichTextContentField()
        {
            NodeType = NODE_TYPE,
            Value = value,
            Marks = [new RichTextMarkField() { Type = boldType }],
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
            Classes = testClasses,
        };

        var renderer = BuildSut([boldMarkOption]);
        var rendererCollection = new RichTextRenderer(
            new NullLogger<RichTextRenderer>(),
            new[] { renderer }
        );

        const string value = "Paragraph text";

        var content = new RichTextContentField() { NodeType = NODE_TYPE, Value = value };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Equal(value, html);
    }

    [Fact]
    public void Should_Log_On_Missing_Mark_Option()
    {
        var boldMarkField = new RichTextMarkField() { Type = "bold" };

        var renderer = BuildSut();
        var rendererCollection = new RichTextRenderer(
            new NullLogger<RichTextRenderer>(),
            [renderer]
        );

        var content = new RichTextContentField()
        {
            Marks = [boldMarkField],
            NodeType = NODE_TYPE,
            Value = "Paragraph text",
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        _logger
            .Received(1)
            .Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<IReadOnlyList<KeyValuePair<string, object>>>(state =>
                    state.Any(kv =>
                        kv.Key == "{OriginalFormat}"
                        && (string)kv.Value == "Missing mark option for {Mark}"
                    )
                    && state.Any(kv => kv.Key == "Mark" && ReferenceEquals(kv.Value, boldMarkField))
                ),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }
}
