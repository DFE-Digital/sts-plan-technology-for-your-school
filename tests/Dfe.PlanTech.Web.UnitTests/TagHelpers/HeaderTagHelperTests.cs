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
    [Fact]
    public void Should_RenderH1Tag_When_H1()
    {
        var header = new Header()
        {
            Text = "Header text",
            Tag = HeaderTag.H1,
            Size = HeaderSize.Small
        };

        var loggerMock = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerMock)
        {
            Model = header
        };

        var html = tagHelper.GetHtml();

        Assert.StartsWith("<H1", html);
        Assert.Contains(header.Text, html);
        Assert.EndsWith("</H1>", html);
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

        var loggerMock = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerMock)
        {
            Model = header
        };

        var html = tagHelper.GetHtml();

        Assert.StartsWith("<H2", html);
        Assert.Contains(header.Text, html);
        Assert.EndsWith("</H2>", html);
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

        var loggerMock = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerMock)
        {
            Model = header
        };

        var html = tagHelper.GetHtml();

        Assert.StartsWith("<H3", html);
        Assert.Contains(header.Text, html);
        Assert.EndsWith("</H3>", html);
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

        var loggerMock = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerMock)
        {
            Model = header
        };

        var html = tagHelper.GetHtml();

        var expectedClass = header.GetClassForSize();

        Assert.Contains($" class=\"{expectedClass}\">", html);
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

        var loggerMock = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerMock)
        {
            Model = header
        };

        var html = tagHelper.GetHtml();

        var expectedClass = header.GetClassForSize();

        Assert.Contains($" class=\"{expectedClass}\">", html);
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

        var loggerMock = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerMock)
        {
            Model = header
        };

        var html = tagHelper.GetHtml();

        var expectedClass = header.GetClassForSize();

        Assert.Contains($" class=\"{expectedClass}\">", html);
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

        var loggerMock = Substitute.For<ILogger<HeaderComponentTagHelper>>();
        var tagHelper = new HeaderComponentTagHelper(loggerMock)
        {
            Model = header
        };

        var html = tagHelper.GetHtml();

        var expectedClass = header.GetClassForSize();

        Assert.Contains($" class=\"{expectedClass}\">", html);
    }

    [Fact]
    public async Task Should_LogWarning_When_Model_Is_Null()
    {
        var content = "content";

        var loggerMock = Substitute.For<ILogger<HeaderComponentTagHelper>>();

        var tagHelper = new HeaderComponentTagHelper(loggerMock){};

        var context = new TagHelperContext(tagName: "header",
                                            allAttributes: new TagHelperAttributeList(),
                                            items: new Dictionary<object, object>(),
                                            uniqueId: "header-test");

        var output = new TagHelperOutput("header-tag",
                                        attributes: new TagHelperAttributeList(),
                                        getChildContentAsync: (useCachedResult, encoder) =>
                                        {
                                            var tagHelperContent = new DefaultTagHelperContent();
                                            tagHelperContent.SetContent(content);
                                            return Task.FromResult<TagHelperContent>(tagHelperContent);
                                        });

        await tagHelper.ProcessAsync(context, output);

        loggerMock.HadMethodCalled("Log");
    }

    [Fact]
    public async Task Should_LogWarning_When_HeaderTag_Is_Unknown()
    {
        var content = "content";

        var loggerMock = Substitute.For<ILogger<HeaderComponentTagHelper>>();

        var tagHelper = new HeaderComponentTagHelper(loggerMock)
        {
            Model = new Header()
            {
                Tag = HeaderTag.Unknown
            }
        };

        var context = new TagHelperContext(tagName: "header",
                                            allAttributes: new TagHelperAttributeList(),
                                            items: new Dictionary<object, object>(),
                                            uniqueId: "header-test");

        var output = new TagHelperOutput("header-tag",
                                        attributes: new TagHelperAttributeList(),
                                        getChildContentAsync: (useCachedResult, encoder) =>
                                        {
                                            var tagHelperContent = new DefaultTagHelperContent();
                                            tagHelperContent.SetContent(content);
                                            return Task.FromResult<TagHelperContent>(tagHelperContent);
                                        });

        await Assert.ThrowsAnyAsync<Exception>(() => tagHelper.ProcessAsync(context, output));

        loggerMock.HadMethodCalled("Log");
    }

}