using System.ComponentModel;
using Dfe.PlanTech.Core.Contentful.Enums;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.UnitTests.Shared.Extensions;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers;

public class HeaderComponentTagHelperTests
{
    private readonly ILogger<HeaderComponentTagHelper> _loggerSubstitute;
    private HeaderComponentTagHelper? _tagHelper;
    private readonly TagHelperContext _context;
    private readonly TagHelperOutput _output;

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
            uniqueId: "header-test"
        );
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
            }
        );
    }

    [Theory]
    [InlineData(HeaderTag.H1, "h1", HeaderSize.Small)]
    [InlineData(HeaderTag.H2, "h2", HeaderSize.Small)]
    [InlineData(HeaderTag.H3, "h3", HeaderSize.Small)]
    [InlineData(HeaderTag.H1, "h1", HeaderSize.Medium)]
    [InlineData(HeaderTag.H1, "h1", HeaderSize.Large)]
    [InlineData(HeaderTag.H1, "h1", HeaderSize.ExtraLarge)]
    public void Should_Render_Valid_Tag_And_Class(
        HeaderTag headerTag,
        string expectedTag,
        HeaderSize headerSize
    )
    {
        var header = new ComponentHeaderEntry()
        {
            Text = "Header text",
            Tag = headerTag,
            Size = headerSize,
        };

        _tagHelper = new HeaderComponentTagHelper(_loggerSubstitute) { Model = header };

        _tagHelper.Process(_context, _output);

        var expectedClass = header.GetClassForSize();

        Assert.Equal(expectedTag, _output.TagName);
        Assert.Contains(header.Text, _output.Content.GetContent());
        Assert.Contains($"{expectedClass}", _output.Attributes["class"].Value.ToString());
    }

    [Fact]
    public async Task Should_LogWarning_When_Model_Is_Null()
    {
        var tagHelper = new HeaderComponentTagHelper(_loggerSubstitute);

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        await tagHelper.ProcessAsync(context, output);

        var logMessage = _loggerSubstitute.ReceivedLogMessages().FirstOrDefault();
        Assert.NotNull(logMessage?.Message);
        Assert.Contains($"Missing or invalid HeaderTag {tagHelper.Model}", logMessage.Message);
        Assert.Equal(LogLevel.Warning, logMessage.LogLevel);
    }

    [Fact]
    public async Task Should_LogWarning_When_HeaderTag_Is_Unknown()
    {
        var header = new ComponentHeaderEntry() { Text = "Header text", Tag = HeaderTag.Unknown };

        _tagHelper = new HeaderComponentTagHelper(_loggerSubstitute) { Model = header };

        await _tagHelper.ProcessAsync(_context, _output);

        var logMessage = _loggerSubstitute.ReceivedLogMessages().FirstOrDefault();

        Assert.NotNull(logMessage?.Message);
        Assert.Contains($"Missing or invalid HeaderTag {_tagHelper.Model}", logMessage.Message);
        Assert.Equal(LogLevel.Warning, logMessage.LogLevel);
    }

    [Fact]
    public async Task Should_Throw_Exception_When_HeaderSize_Is_Unknown()
    {
        var header = new ComponentHeaderEntry()
        {
            Text = "Header text",
            Tag = HeaderTag.H1,
            Size = HeaderSize.Unknown,
        };

        _tagHelper = new HeaderComponentTagHelper(_loggerSubstitute) { Model = header };

        var exception = await Assert.ThrowsAsync<InvalidEnumArgumentException>(async () =>
            await _tagHelper.ProcessAsync(_context, _output)
        );

        Assert.Equal($"Could not find header size for {header.Size}", exception.Message);
    }
}
