namespace Dfe.PlanTech.Web.UnitTests.TagHelpers;

public class FooterLinkTagHelperTests
{
    private readonly FooterLinkTagHelper _tagHelper;
    private readonly TagHelperContext _context;
    private readonly TagHelperOutput _output;
    public FooterLinkTagHelperTests()
    {
        var logger = Substitute.For<ILogger<FooterLinkTagHelper>>();
        _tagHelper = new FooterLinkTagHelper(logger);

        _context = new TagHelperContext(tagName: "footer-link",
                                                    allAttributes: new TagHelperAttributeList(),
                                                    items: new Dictionary<object, object>(),
                                                    uniqueId: "footer-link-test");

        _output = new TagHelperOutput("footer-link-tag",
                                        attributes: new TagHelperAttributeList(),
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

        var html = _output.Content.ToHtmlString();

        Assert.StartsWith("<a", html);
        Assert.Contains($"href=\"{link.Href}\"", html);
        Assert.Contains($">{link.DisplayText}<", html);
        Assert.EndsWith("</a>", html);

        var targetHtml = " target=\"_blank\"";
        if (link.OpenInNewTab)
        {
            Assert.Contains(targetHtml, html);
        }
        else
        {
            Assert.DoesNotContain(targetHtml, html);
        }
    }

    [Fact]
    public async Task Should_Log_Message_And_Render_Nothing_When_Link_Null()
    {
        await _tagHelper.ProcessAsync(_context, _output);

        var html = _output.Content.ToHtmlString();

        Assert.Empty(html);
    }
}
