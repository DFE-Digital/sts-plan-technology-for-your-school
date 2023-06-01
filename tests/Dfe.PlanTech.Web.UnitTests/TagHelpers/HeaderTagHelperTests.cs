using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.TagHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.TagHelpers;

public class HeaderTagHelperTests
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

        var loggerMock = new Mock<ILogger<HeaderTagHelper>>();
        var tagHelper = new HeaderTagHelper(loggerMock.Object)
        {
            Header = header
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

        var loggerMock = new Mock<ILogger<HeaderTagHelper>>();
        var tagHelper = new HeaderTagHelper(loggerMock.Object)
        {
            Header = header
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

        var loggerMock = new Mock<ILogger<HeaderTagHelper>>();
        var tagHelper = new HeaderTagHelper(loggerMock.Object)
        {
            Header = header
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

        var loggerMock = new Mock<ILogger<HeaderTagHelper>>();
        var tagHelper = new HeaderTagHelper(loggerMock.Object)
        {
            Header = header
        };

        var html = tagHelper.GetHtml();

        var expectedClass = header.GetClassForSize();

        Assert.Contains($" class=\"{expectedClass}>", html);
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

        var loggerMock = new Mock<ILogger<HeaderTagHelper>>();
        var tagHelper = new HeaderTagHelper(loggerMock.Object)
        {
            Header = header
        };

        var html = tagHelper.GetHtml();

        var expectedClass = header.GetClassForSize();

        Assert.Contains($" class=\"{expectedClass}>", html);
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

        var loggerMock = new Mock<ILogger<HeaderTagHelper>>();
        var tagHelper = new HeaderTagHelper(loggerMock.Object)
        {
            Header = header
        };

        var html = tagHelper.GetHtml();

        var expectedClass = header.GetClassForSize();

        Assert.Contains($" class=\"{expectedClass}>", html);
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

        var loggerMock = new Mock<ILogger<HeaderTagHelper>>();
        var tagHelper = new HeaderTagHelper(loggerMock.Object)
        {
            Header = header
        };

        var html = tagHelper.GetHtml();

        var expectedClass = header.GetClassForSize();

        Assert.Contains($" class=\"{expectedClass}>", html);
    }
}