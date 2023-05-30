using System.Text;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Interfaces;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Models.PartRenderers;
using Dfe.PlanTech.Infrastructure.Contentful.Content.Renderers.Options;
using Moq;

namespace Dfe.PlanTech.Infrastructure.Contentful.UnitTests.Content.Renderers.Models;

public class RichTextRendererTests
{
    private readonly RichTextRenderer _renderer;
    private readonly Mock<IRichTextContentPartRenderer> _partRenderer = new();

    public RichTextRendererTests()
    {
        _partRenderer.Setup(partRenderer => partRenderer.Accepts(It.IsAny<IRichTextContent>()))
                    .Returns((IRichTextContent content) => content.NodeType == "paragraph");

        _partRenderer.Setup(partRenderer => partRenderer.AddHtml(It.IsAny<IRichTextContent>(), It.IsAny<IRichTextContentPartRendererCollection>(), It.IsAny<StringBuilder>()))
                    .Returns((IRichTextContent content, IRichTextContentPartRendererCollection _, StringBuilder stringBuilder) =>
                    {
                        stringBuilder.Append(content.Value);

                        return stringBuilder;
                    })
                    .Verifiable();

        var partRenderers = new List<IRichTextContentPartRenderer>(){
            _partRenderer.Object,
            new HyperlinkRenderer(new HyperlinkRendererOptions())
        };

        _renderer = new RichTextRenderer(partRenderers);
    }

    [Fact]
    public void Should_RetrieveCorrectRenderer_WhenParagraph()
    {
        var content = new RichTextContent()
        {
            NodeType = "paragraph",
        };

        var renderer = _renderer.GetRendererForContent(content);

        Assert.Equal(renderer, _partRenderer.Object);
    }

    [Fact]
    public void Should_CreateHtml_When_PassedContent()
    {
        const string testHtml = "testing";

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
        _partRenderer.Verify(renderer => renderer.AddHtml(It.IsAny<IRichTextContent>(), It.IsAny<IRichTextContentPartRendererCollection>(), It.IsAny<StringBuilder>()));

        Assert.Equal(testHtml, html);
    }
}
