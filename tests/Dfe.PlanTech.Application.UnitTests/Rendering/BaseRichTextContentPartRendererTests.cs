using System.Text;
using Dfe.PlanTech.Application.Rendering;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;

namespace Dfe.PlanTech.Application.UnitTests.Rendering;

public class BaseRichTextContentPartRendererTests
{
    private sealed class TestRenderer : BaseRichTextContentPartRenderer
    {
        public TestRenderer(RichTextNodeType nodeType) : base(nodeType) { }
        public override StringBuilder AddHtml(
            RichTextContentField content,
            IRichTextContentPartRendererCollection rendererCollection,
            StringBuilder sb
        ) => sb;
    }

    private sealed class TestContent : IRichTextContent
    {
        public TestContent(RichTextNodeType mapped) => MappedNodeType = mapped;
        public RichTextNodeType MappedNodeType { get; }
        public string Value { get; set; } = null!;
        public string NodeType { get; set; } = null!;
    }

    public static IEnumerable<object[]> AllRichTextNodeTypes()
    {
        foreach (var value in Enum.GetValues(typeof(RichTextNodeType)))
        {
            yield return new object[] { value, true };
        }
    }

    [Theory]
    [InlineData(RichTextNodeType.Paragraph, RichTextNodeType.Paragraph, true)]
    [InlineData(RichTextNodeType.Paragraph, RichTextNodeType.Table, false)]
    public void Accepts_Matches_Only_When_Types_Equal(RichTextNodeType rendererType, RichTextNodeType contentType, bool expected)
    {
        var renderer = new TestRenderer(rendererType);
        var content = new TestContent(contentType);

        Assert.Equal(expected, renderer.Accepts(content));
    }
}
