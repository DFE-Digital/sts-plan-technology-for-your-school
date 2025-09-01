using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.TagHelpers.RichText;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers.RichText;

public class RichTextTagHelperTests
{
    [Fact]
    public async Task Should_LogWarning_When_Content_Is_Null()
    {
        string content = "rich text tag";

        var loggerSubstitute = Substitute.For<ILogger<RichTextTagHelper>>();
        var richTextRendererSubstitute = Substitute.For<IRichTextRenderer>();

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

        var richTextTagHelper = new RichTextTagHelper(loggerSubstitute, richTextRendererSubstitute);

        await richTextTagHelper.ProcessAsync(context, output);
        Assert.Equal("rich-text-tag", output.TagName);
        Assert.Equal("richtext-test", context.UniqueId);
    }

    [Fact]
    public async Task Should_SetHtml_When_Content_Exists()
    {
        string content = "rich text tag";
        string expectedHtml = "has executed";

        var loggerSubstitute = Substitute.For<ILogger<RichTextTagHelper>>();
        var richTextRendererSubstitute = Substitute.For<IRichTextRenderer>();
        richTextRendererSubstitute.ToHtml(Arg.Any<RichTextContentField>()).Returns(expectedHtml);

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

        var richTextTagHelper = new RichTextTagHelper(loggerSubstitute, richTextRendererSubstitute)
        {
            Content = new RichTextContentField()
            {
                Value = expectedHtml
            }
        };

        await richTextTagHelper.ProcessAsync(context, output);

        Assert.NotNull(richTextTagHelper.Content);
        Assert.Equal(expectedHtml, richTextTagHelper.Content.Value);
    }
}
