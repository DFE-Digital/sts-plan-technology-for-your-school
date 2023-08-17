using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.TagHelpers.RichText;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers;

public class RichTextTagHelperTests
{
    [Fact]
    public async Task Should_LogWarning_When_Content_Is_Null()
    {
        string content = "rich text tag";

        var loggerMock = Substitute.For<ILogger<RichTextTagHelper>>();
        var richTextRendererMock = Substitute.For<IRichTextRenderer>();

        var context = new TagHelperContext(tagName: "rich-text",
                                                allAttributes: new TagHelperAttributeList(),
                                                items: new Dictionary<object, object>(),
                                                uniqueId: "richtext-test");

        var output = new TagHelperOutput("rich-text-tag",
                                        attributes: new TagHelperAttributeList(),
                                        getChildContentAsync: (useCachedResult, encoder) =>
                                        {
                                            var tagHelperContent = new DefaultTagHelperContent();
                                            tagHelperContent.SetContent(content);
                                            return Task.FromResult<TagHelperContent>(tagHelperContent);
                                        });

        var richTextTagHelper = new RichTextTagHelper(loggerMock, richTextRendererMock);

        await richTextTagHelper.ProcessAsync(context, output);
    }

    [Fact]
    public async Task Should_SetHtml_When_Content_Exists()
    {
        string content = "rich text tag";
        string expectedHtml = "has executed";

        var loggerMock = Substitute.For<ILogger<RichTextTagHelper>>();

        var richTextRendererMock = Substitute.For<IRichTextRenderer>();
        richTextRendererMock.ToHtml(Arg.Any<IRichTextContent>()).Returns(expectedHtml);

        var context = new TagHelperContext(tagName: "rich-text",
                                            allAttributes: new TagHelperAttributeList(),
                                            items: new Dictionary<object, object>(),
                                            uniqueId: "richtext-test");

        var output = new TagHelperOutput("rich-text-tag",
                                        attributes: new TagHelperAttributeList(),
                                        getChildContentAsync: (useCachedResult, encoder) =>
                                        {
                                            var tagHelperContent = new DefaultTagHelperContent();
                                            tagHelperContent.SetContent(content);
                                            return Task.FromResult<TagHelperContent>(tagHelperContent);
                                        });

        var richTextTagHelper = new RichTextTagHelper(loggerMock, richTextRendererMock)
        {
            Content = new RichTextContent()
            {
                Value = expectedHtml
            }
        };

        await richTextTagHelper.ProcessAsync(context, output);

        Assert.NotNull(richTextTagHelper.Content);
        Assert.Equal(expectedHtml, richTextTagHelper.Content.Value);
    }
}