using System.Reflection;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport;
using Dfe.PlanTech.Domain.Content.Models.ContentSupport.Mapped;
using Dfe.PlanTech.Web.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers;

public class FooterLinkTagHelperTests
{
    private readonly FooterLinkTagHelper _tagHelper;
    private readonly TagHelperContext _context;
    private readonly TagHelperOutput _output;
    private readonly ILogger<FooterLinkTagHelper> logger = Substitute.For<ILogger<FooterLinkTagHelper>>();

    public FooterLinkTagHelperTests()
    {
        _tagHelper = new FooterLinkTagHelper(logger);

        _context = new TagHelperContext(tagName: "footer-link",
                                                    allAttributes: new TagHelperAttributeList(),
                                                    items: new Dictionary<object, object>(),
                                                    uniqueId: "footer-link-test");

        _output = new TagHelperOutput("footer-link-tag",
                                        attributes: [],
                                        getChildContentAsync: (useCachedResult, encoder) =>
                                        {
                                            var tagHelperContent = new DefaultTagHelperContent();
                                            tagHelperContent.SetContent("");
                                            return Task.FromResult<TagHelperContent>(tagHelperContent);
                                        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Render_Anchor_Link_When_Valid(bool openInNewTab)
    {
        var link = new NavigationLink()
        {
            DisplayText = "Click me",
            Href = "www.click-me.com",
            OpenInNewTab = openInNewTab,
        };

        _tagHelper.Link = link;

        await _tagHelper.ProcessAsync(_context, _output);

        Assert.Equal("a", _output.TagName);
        Assert.Equal(link.Href, _output.Attributes["href"].Value);

        if (link.OpenInNewTab)
        {
            Assert.Equal("_blank", _output.Attributes["target"].Value);
        }
        else
        {
            Assert.Null(_output.Attributes["target"]);
        }
    }

    [Fact]
    public async Task Should_Log_Message_And_Render_Nothing_When_Link_Null()
    {
        await _tagHelper.ProcessAsync(_context, _output);

        Assert.Empty(_output.Attributes);
    }

    [Theory]
    [InlineData("/test-page", "test-page", typeof(Page))]
    [InlineData("/content/test-page", "test-page", typeof(CsPage))]
    [InlineData("/content/test-page", "test-page", typeof(ContentSupportPage))]
    [InlineData("/content/prefixed-slash", "/prefixed-slash", typeof(ContentSupportPage))]
    [InlineData("/content/prefixed-slash", "/prefixed-slash", typeof(CsPage))]
    [InlineData("/prefixed-slash", "/prefixed-slash", typeof(Page))]
    public async Task Should_Render_Correct_Url_For_ContentToLinkTo(string expectedUrl, string slug, Type contentType)
    {
        IContentComponent content = contentType switch
        {
            var t when t == typeof(Page) => new Page { Slug = slug },
            var t when t == typeof(CsPage) => new CsPage { Slug = slug },
            _ => new ContentSupportPage { Slug = slug }
        };

        var link = new NavigationLink()
        {
            DisplayText = "Content link",
            ContentToLinkTo = content
        };

        _tagHelper.Link = link;

        await _tagHelper.ProcessAsync(_context, _output);

        Assert.Equal(expectedUrl, _output.Attributes["href"].Value);
    }

    [Fact]
    public async Task Should_Log_Error_And_Return_Empty_When_Invalid_Content_Type()
    {
        var invalidContent = Substitute.For<IContentComponent>();
        var link = new NavigationLink()
        {
            DisplayText = "Invalid content",
            ContentToLinkTo = invalidContent
        };

        _tagHelper.Link = link;

        await _tagHelper.ProcessAsync(_context, _output);

        Assert.Empty(_output.Attributes);
    }

    [Fact]
    public async Task Should_Return_Empty_When_No_Href_Or_Content()
    {
        var link = new NavigationLink()
        {
            DisplayText = "Empty link"
        };

        _tagHelper.Link = link;

        await _tagHelper.ProcessAsync(_context, _output);

        Assert.Empty(_output.Attributes);
        Assert.Null(_output.TagName);
    }

    [Fact]
    public void Should_Log_Error_When_No_Href_Or_Content()
    {
        var link = new NavigationLink()
        {
            DisplayText = "Empty"
        };

        _tagHelper.Link = link;
        var methodInfo = typeof(FooterLinkTagHelper).GetMethod("GetHref", BindingFlags.NonPublic | BindingFlags.Instance);

        methodInfo?.Invoke(_tagHelper, []);

        var logMessage = logger.ReceivedLogMessages().FirstOrDefault(msg => msg.LogLevel == LogLevel.Error && msg.Message.Contains("No href or content to link to for"));
        Assert.NotNull(logMessage);
    }
}
