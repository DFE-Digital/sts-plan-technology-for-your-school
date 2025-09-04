using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Data.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Dfe.PlanTech.Data.Contentful.UnitTests.Content.Renderers.Models;

public class RichTextRendererTests
{
    private readonly RichTextRenderer _renderer;
    private readonly IRichTextContentPartRenderer _partRenderer = Substitute.For<IRichTextContentPartRenderer>();

    public RichTextRendererTests()
    {
        _partRenderer.Accepts(Arg.Any<RichTextContent>())
                    .Returns((content) => ((IRichTextContent)content[0]).NodeType == "paragraph");

        _partRenderer.AddHtml(Arg.Any<RichTextContent>(), Arg.Any<IRichTextContentPartRendererCollection>(), Arg.Any<StringBuilder>())
                    .Returns((CallInfo) =>
                    {
                        IRichTextContent content = (IRichTextContent)CallInfo[0];
                        IRichTextContentPartRendererCollection collection = (IRichTextContentPartRendererCollection)CallInfo[1];
                        StringBuilder stringBuilder = (StringBuilder)CallInfo[2];

                        stringBuilder.Append(content.Value);

                        return stringBuilder;
                    });

        var partRenderers = new List<IRichTextContentPartRenderer>(){
            _partRenderer,
            new HyperlinkRenderer(new HyperlinkRendererOptions())
        };

        _renderer = new RichTextRenderer(new NullLogger<RichTextRenderer>(), partRenderers);
    }

    [Fact]
    public void Should_RetrieveCorrectRenderer_WhenParagraph()
    {
        var content = new RichTextContent()
        {
            NodeType = "paragraph",
        };

        var renderer = _renderer.GetRendererForContent(content);

        Assert.Equal(renderer, _partRenderer);
    }

    [Fact]
    public void Should_CreateHtml_When_PassedContent()
    {
        var testHtml = "testing";

        var content = new RichTextContent()
        {
            NodeType = "document",
            Content = new() {
                new RichTextContent() {
                    NodeType = "paragraph",
                    Value = testHtml
                }
            }
        };

        var html = _renderer.ToHtml(content);
        _partRenderer.Received().AddHtml(Arg.Any<RichTextContent>(), Arg.Any<IRichTextContentPartRendererCollection>(), Arg.Any<StringBuilder>());

        Assert.Equal(testHtml, html);
    }

}
