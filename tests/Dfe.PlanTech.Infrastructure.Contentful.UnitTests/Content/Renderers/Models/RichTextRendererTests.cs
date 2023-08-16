using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Text;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models;

public class RichTextRendererTests
{
    private readonly RichTextRenderer _renderer;
    private IRichTextContentPartRenderer _partRenderer = Substitute.For<IRichTextContentPartRenderer>();

    public RichTextRendererTests()
    {
        _partRenderer.Accepts(Arg.Any<IRichTextContent>())
                    .Returns((IRichTextContent content) => content.NodeType == "paragraph");



        _partRenderer.AddHtml(Arg.Any<IRichTextContent>(), Arg.Any<IRichTextContentPartRendererCollection>(), Arg.Any<StringBuilder>())
                    .Returns((IRichTextContent content, IRichTextContentPartRendererCollection collection, StringBuilder stringBuilder) =>
                    {
                        stringBuilder.Append(content.Value);

                        return stringBuilder;
                    })
                    .Verifiable();

        var partRenderers = new List<IRichTextContentPartRenderer>(){
            _partRenderer,
            new HyperlinkRenderer(new HyperlinkRendererOptions())
        };

        _renderer = new RichTextRenderer(new NullLogger<IRichTextRenderer>(), partRenderers);
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
            Content = new[] {
                new RichTextContent() {
                    NodeType = "paragraph",
                    Value = testHtml
                }
            }
        };

        var html = _renderer.ToHtml(content);
        _partRenderer.Received().AddHtml(Arg.Any<IRichTextContent>(), Arg.Any<IRichTextContentPartRendererCollection>(), Arg.Any<StringBuilder>());

        Assert.Equal(testHtml, html);
    }

}
