using System.ComponentModel;
using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers;

public class HeaderComponentTagHelperTests
{
    private ILogger<HeaderComponentTagHelper> _loggerSubstitute;
    private HeaderComponentTagHelper? _tagHelper;
    private TagHelperContext _context;
    private TagHelperOutput _output;

    public HeaderComponentTagHelperTests()
    {
        _loggerSubstitute = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        _context = CreateTagHelperContext();
        _output = CreateTagHelperOutput();
    }


    private TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            tagName: "header",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "header-test");
    }

    private TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "header",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetContent("");
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    [Theory]
    [InlineData(HeaderTag.H1, "h1", HeaderSize.Small)]
    [InlineData(HeaderTag.H2, "h2", HeaderSize.Small)]
    [InlineData(HeaderTag.H3, "h3", HeaderSize.Small)]
    [InlineData(HeaderTag.H1, "h1", HeaderSize.Medium)]
    [InlineData(HeaderTag.H1, "h1", HeaderSize.Large)]
    [InlineData(HeaderTag.H1, "h1", HeaderSize.ExtraLarge)]
    public void Should_Render_Valid_Tag_And_Class(HeaderTag headerTag, string expectedTag, HeaderSize headerSize)
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = headerTag,
            Size = headerSize
        };

        _tagHelper = new HeaderComponentTagHelper(_loggerSubstitute)
        {
            Model = header
        };

        _tagHelper.Process(_context, _output);

        var expectedClass = header.GetClassForSize();

        Assert.Equal(expectedTag, _output.TagName);
        Assert.Contains(header.Text, _output.Content.GetContent());
        Assert.Contains($"{expectedClass}", _output.Attributes["class"].Value.ToString());
    }

    [Fact]
    public async Task Should_LogWarning_When_Model_Is_Null()
    {
        var loggerSubstitute = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerSubstitute);

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        await tagHelper.ProcessAsync(context, output);

        var logMessage = loggerSubstitute.ReceivedLogMessages().FirstOrDefault();
        Assert.NotNull(logMessage?.Message);
        Assert.Contains($"Missing or invalid Header {tagHelper.Model}", logMessage.Message);
        Assert.Equal(LogLevel.Warning, logMessage.LogLevel);
    }

    [Fact]
    public async Task Should_LogWarning_When_HeaderTag_Is_Unknown()
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = HeaderTag.Unknown
        };

        _tagHelper = new HeaderComponentTagHelper(_loggerSubstitute)
        {
            Model = header
        };

        await _tagHelper.ProcessAsync(_context, _output);

        var logMessage = _loggerSubstitute.ReceivedLogMessages().FirstOrDefault();

        Assert.NotNull(logMessage?.Message);
        Assert.Contains($"Missing or invalid Header {_tagHelper.Model}", logMessage.Message);
        Assert.Equal(LogLevel.Warning, logMessage.LogLevel);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_HeaderSize_Is_Unknown()
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = HeaderTag.H1,
            Size = HeaderSize.Unknown
        };

        _tagHelper = new HeaderComponentTagHelper(_loggerSubstitute)
        {
            Model = header
        };

        var exception = await Assert.ThrowsAsync<InvalidEnumArgumentException>(async () => await _tagHelper.ProcessAsync(_context, _output));

        Assert.Equal($"Could not find header size for {header.Size}", exception.Message);
    }
}
