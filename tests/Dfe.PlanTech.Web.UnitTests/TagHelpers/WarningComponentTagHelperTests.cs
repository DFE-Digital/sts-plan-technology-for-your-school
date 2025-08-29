using Dfe.PlanTech.Web.TagHelpers;
using Dfe.PlanTech.Web.UnitTests.TestHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers;

public class WarningComponentTagHelperTests
{
    [Fact]
    public async Task Should_Create_Tags()
    {
        var content = "test content";

        var context = new TagHelperContext(tagName: "warning-component",
                                                allAttributes: new TagHelperAttributeList(),
                                                items: new Dictionary<object, object>(),
                                                uniqueId: "warning-component-test");

        var output = new TagHelperOutput("warning-component",
                                        attributes: new TagHelperAttributeList(),
                                        getChildContentAsync: (useCachedResult, encoder) =>
                                        {
                                            var tagHelperContent = new DefaultTagHelperContent();
                                            tagHelperContent.SetContent(content);
                                            return Task.FromResult<TagHelperContent>(tagHelperContent);
                                        });

        var tagHelper = new WarningComponentTagHelper();

        await tagHelper.ProcessAsync(context, output);

        var htmlString = output.ToHtmlString();

        Assert.NotNull(htmlString);
        Assert.Contains(content, htmlString);

        Assert.Contains(WarningComponentTagHelper.OpeningDiv, htmlString);
        Assert.Contains(WarningComponentTagHelper.WarningIcon, htmlString);
        Assert.Contains(WarningComponentTagHelper.OpeningSpan, htmlString);
        Assert.Contains(WarningComponentTagHelper.AssistiveText, htmlString);
    }

}
