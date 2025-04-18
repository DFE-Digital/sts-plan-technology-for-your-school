using System.Text;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models.PartRenderers;

public class HyperlinkRendererTests
{
    private const string NODE_TYPE = "hyperlink";

    [Fact]
    public void Should_Accept_When_ContentIsHyperLink()
    {
        const string linkText = "Click Here";
        const string url = "https://www.test-url.com";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = linkText,
            Data = new RichTextContentSupportData()
            {
                Uri = url
            }
        };

        var renderer = new HyperlinkRenderer(new HyperlinkRendererOptions());

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

        var renderer = new HyperlinkRenderer(new HyperlinkRendererOptions());

        var accepted = renderer.Accepts(content);

        Assert.False(accepted);
    }

    [Fact]
    public void Should_Create_Link_For_New_Tab_When_PassedValidExternalLink()
    {
        var renderer = new HyperlinkRenderer(new HyperlinkRendererOptions());
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string linkText = "Click Here";
        const string url = "https://www.test-url.com";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = linkText,
            Data = new RichTextContentSupportData()
            {
                Uri = url,
                Target = new RichTextContentData(),
            }
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Contains($"<a href=\"{url}\"", html);
        Assert.Contains("target=\"_blank\" rel=\"noopener\"", html);
        Assert.Contains($">{linkText} (opens in new tab)</a>", html);
    }

    [Fact]
    public void Should_Create_Link_For_Same_Tab_When_PassedValidInternalLink()
    {
        var renderer = new HyperlinkRenderer(new HyperlinkRendererOptions());
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string linkText = "Click Here";
        const string url = "content/hello-world";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = linkText,
            Data = new RichTextContentSupportData()
            {
                Uri = url,
                Target = new RichTextContentData(),
            }
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Contains($"<a href=\"{url}\"", html);
        Assert.DoesNotContain("target=\"_blank\" rel=\"noopener\"", html);
        Assert.Contains($">{linkText}</a>", html);
    }

    [Fact]
    public void Should_NotAddLink_When_MissingURI()
    {
        var renderer = new HyperlinkRenderer(new HyperlinkRendererOptions());
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string linkText = "Click Here";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = linkText,
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Empty(html);
    }

    [Fact]
    public void Should_AddClasses_When_OptionsContainsClasses()
    {
        const string testClasses = "testing-classes";

        var renderer = new HyperlinkRenderer(new HyperlinkRendererOptions() { Classes = testClasses });
        var rendererCollection = new RichTextRenderer(new NullLogger<RichTextRenderer>(), new[] { renderer });

        const string linkText = "Click Here";
        const string url = "https://www.test-url.com";

        var content = new RichTextContent()
        {
            NodeType = NODE_TYPE,
            Value = linkText,
            Data = new RichTextContentSupportData()
            {
                Uri = url,
                Target = new RichTextContentData(),
            }
        };

        var result = renderer.AddHtml(content, rendererCollection, new StringBuilder());

        var html = result.ToString();

        Assert.Contains($"class=\"{testClasses}\"", html);
    }

}
