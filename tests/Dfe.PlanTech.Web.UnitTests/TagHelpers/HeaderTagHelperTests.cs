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

    [Fact]
    public void Should_RenderH1Tag_When_H1()
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = HeaderTag.H1,
            Size = HeaderSize.Small
        };

        var loggerSubstitute = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerSubstitute)
        {
            Model = header
        };

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        Assert.Equal("h1", output.TagName);
        Assert.Contains(header.Text, output.Content.GetContent());
    }

    [Fact]
    public void Should_RenderH2Tag_When_H2()
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = HeaderTag.H2,
            Size = HeaderSize.Small
        };

        var loggerSubstitute = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerSubstitute)
        {
            Model = header
        };

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        Assert.Equal("h2", output.TagName);
        Assert.Contains(header.Text, output.Content.GetContent());
    }

    [Fact]
    public void Should_RenderH3Tag_When_H3()
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = HeaderTag.H3,
            Size = HeaderSize.Small
        };

        var loggerSubstitute = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerSubstitute)
        {
            Model = header
        };

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        Assert.Equal("h3", output.TagName);
        Assert.Contains(header.Text, output.Content.GetContent());
    }

    [Fact]
    public void Should_RenderRightClass_When_SmallSize()
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = HeaderTag.H1,
            Size = HeaderSize.Small
        };

        var loggerSubstitute = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerSubstitute)
        {
            Model = header
        };

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var expectedClass = header.GetClassForSize();

        Assert.Contains($"{expectedClass}", output.Attributes["class"].Value.ToString());
    }

    [Fact]
    public void Should_RenderRightClass_When_MediumSize()
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = HeaderTag.H1,
            Size = HeaderSize.Medium
        };

        var loggerSubstitute = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerSubstitute)
        {
            Model = header
        };

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var expectedClass = header.GetClassForSize();

        Assert.Contains($"{expectedClass}", output.Attributes["class"].Value.ToString());
    }

    [Fact]
    public void Should_RenderRightClass_When_LargeSize()
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = HeaderTag.H1,
            Size = HeaderSize.Large
        };

        var loggerSubstitute = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerSubstitute)
        {
            Model = header
        };

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var expectedClass = header.GetClassForSize();

        Assert.Contains($"{expectedClass}", output.Attributes["class"].Value.ToString());
    }

    [Fact]
    public void Should_RenderRightClass_When_ExtraLarge()
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = HeaderTag.H1,
            Size = HeaderSize.ExtraLarge
        };

        var loggerSubstitute = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerSubstitute)
        {
            Model = header
        };

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        tagHelper.Process(context, output);

        var expectedClass = header.GetClassForSize();

        Assert.Contains($"{expectedClass}", output.Attributes["class"].Value.ToString());
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

        var loggerSubstitute = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerSubstitute)
        {
            Model = header
        };

        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        await tagHelper.ProcessAsync(context, output);

        var logMessage = loggerSubstitute.ReceivedLogMessages().FirstOrDefault();

        Assert.NotNull(logMessage?.Message);
        Assert.Contains($"Missing or invalid Header {tagHelper.Model}", logMessage.Message);
        Assert.Equal(LogLevel.Warning, logMessage.LogLevel);
    }
}
