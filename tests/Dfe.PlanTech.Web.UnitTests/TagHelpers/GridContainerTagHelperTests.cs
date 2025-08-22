using Dfe.PlanTech.Core.Contentful.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Web.TagHelpers.RichText;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers;

public class GridContainerTagHelperTests
{
    [Fact]
    public async Task Should_SetHtml_When_Content_Exists()
    {
        string content = "grid container tag";
        string expectedHtml = "has executed";

        var loggerSubstitute = Substitute.For<ILogger<GridContainerTagHelper>>();
        var cardContainerRenderer = Substitute.For<ICardContainerContentPartRenderer>();
        cardContainerRenderer.ToHtml(Arg.Any<IReadOnlyList<ComponentCardEntry>>()).Returns(expectedHtml);

        var context = new TagHelperContext(tagName: "grid-container",
                                            allAttributes: new TagHelperAttributeList(),
                                            items: new Dictionary<object, object>(),
                                            uniqueId: "gridcontainer-test");

        var output = new TagHelperOutput("grid-container-tag",
                                        attributes: new TagHelperAttributeList(),
                                        getChildContentAsync: (useCachedResult, encoder) =>
                                        {
                                            var tagHelperContent = new DefaultTagHelperContent();
                                            tagHelperContent.SetContent(content);
                                            return Task.FromResult<TagHelperContent>(tagHelperContent);
                                        });

        var gridContainerTagHelper = new GridContainerTagHelper(loggerSubstitute, cardContainerRenderer)
        {
            Content = [
                new ComponentCardEntry()
                {
                    Description = "this is a test"
                },
                new ComponentCardEntry()
                {
                    Description = "second card test"
                },
            ]
        };

        await gridContainerTagHelper.ProcessAsync(context, output);

        Assert.NotNull(gridContainerTagHelper.Content);
        Assert.Equal(expectedHtml, output.Content.ToHtmlString());
    }

    [Fact]
    public async Task Should_SetHtml_WhenNull_Content_Null()
    {
        string content = "grid container tag";
        string expectedHtml = "has executed";

        var loggerSubstitute = Substitute.For<ILogger<GridContainerTagHelper>>();
        var cardContainerRenderer = Substitute.For<ICardContainerContentPartRenderer>();
        cardContainerRenderer.ToHtml(Arg.Any<IReadOnlyList<ComponentCardEntry>>()).Returns(expectedHtml);

        var context = new TagHelperContext(tagName: "grid-container",
                                            allAttributes: new TagHelperAttributeList(),
                                            items: new Dictionary<object, object>(),
                                            uniqueId: "gridcontainer-test");

        var output = new TagHelperOutput("grid-container-tag",
                                        attributes: new TagHelperAttributeList(),
                                        getChildContentAsync: (useCachedResult, encoder) =>
                                        {
                                            var tagHelperContent = new DefaultTagHelperContent();
                                            tagHelperContent.SetContent(content);
                                            return Task.FromResult<TagHelperContent>(tagHelperContent);
                                        });

        var gridContainerTagHelper = new GridContainerTagHelper(loggerSubstitute, cardContainerRenderer)
        {
            Content = null
        };

        await gridContainerTagHelper.ProcessAsync(context, output);

        Assert.Null(gridContainerTagHelper.Content);
        Assert.NotEqual(expectedHtml, output.Content.ToHtmlString());
    }
}
