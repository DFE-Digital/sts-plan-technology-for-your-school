using System.Xml.Linq;
using Dfe.ContentSupport.Web.Configuration;
using Contentful.Core.Models;
using Dfe.ContentSupport.Web.Models.Mapped;

namespace Dfe.ContentSupport.Web.Tests.Services;

public class ContentServiceTests
{
    private readonly Mock<IContentfulService> _httpContentClientMock = new();
    private readonly Mock<ICacheService<List<CsPage>>> _cacheMock = new();
    private readonly Mock<IModelMapper> _mapperMock = new();

    private readonly ContentfulCollection<ContentSupportPage> _response = new()
    {
        Items = new List<ContentSupportPage>
        {
            new() { Slug = "slug1", IsSitemap = true, SystemProperties = new SystemProperties() },
            new() { Slug = "slug2", IsSitemap = false, SystemProperties = new SystemProperties() },
            new() { Slug = "slug3", IsSitemap = true, SystemProperties = new SystemProperties() }
        }
    };

    private ContentService GetService() => new(_httpContentClientMock.Object, _cacheMock.Object, _mapperMock.Object);
    
    private void SetupResponse(ContentfulCollection<ContentSupportPage>? response = null)
    {
        var res = response ?? _response;

        _httpContentClientMock.Setup(o => o.GetContentSupportPages(It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(res);
        
        _mapperMock.Setup(o => o.MapToCsPages(res))
            .Returns(res.Items
                .Select(page => new ModelMapper(new SupportedAssetTypes()).MapToCsPage(page))
                .ToList());
    }

    [Fact]
    public async Task GetContent_Calls_Client_Once()
    {
        var sut = GetService();
        SetupResponse();
        await sut.GetContent(It.IsAny<string>());

        _httpContentClientMock.Verify(o =>
                o.GetContentSupportPages(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetContent_EmptyResponse_Returns_Null()
    {
        SetupResponse(new ContentfulCollection<ContentSupportPage> { Items = [] });

        var sut = GetService();
        var result = await sut.GetContent(It.IsAny<string>());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetContent_Returns_First_Result()
    {
        SetupResponse();

        var sut = GetService();
        var result = await sut.GetContent(It.IsAny<string>());

        var expected =
            new ModelMapper(new SupportedAssetTypes()).MapToCsPage(_response.Items.First());
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GenerateSitemap_Should_Generate_Expected()
    {
        const string expectedStr =
            """<?xml version="1.0" encoding="UTF-8" standalone="no"?><urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"><url><loc>DUMMY_slug1</loc><changefreq>yearly</changefreq></url><url><loc>DUMMY_slug2</loc><changefreq>yearly</changefreq></url><url><loc>DUMMY_slug3</loc><changefreq>yearly</changefreq></url></urlset>""";
        SetupResponse();

        var expected = XDocument.Parse(expectedStr);
        var sut = GetService();
        var resultStr = await sut.GenerateSitemap("DUMMY_");
        var result = XDocument.Parse(resultStr);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetCsPages_Calls_Client_Once()
    {
        SetupResponse();
        var sut = GetService();
        await sut.GetCsPages();

        _httpContentClientMock.Verify(o =>
                o.GetContentSupportPages(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetCsPages_NotPreview_Calls_Cache_Correct_Key()
    {
        const string expectedKey = "IsSitemap_true";
        SetupResponse();
        var sut = GetService();
        await sut.GetCsPages(false);

        _cacheMock.Verify(o => o.GetFromCache(expectedKey), Times.Once);
    }

    [Fact]
    public async Task GetCsPages_Preview_Calls_Cache_Correct_Key()
    {
        const string expectedKey = "IsSitemap_true";
        SetupResponse();
        var sut = GetService();
        await sut.GetCsPages();

        _cacheMock.Verify(o => o.GetFromCache(expectedKey), Times.Never);
    }

    [Fact]
    public async Task GetCsPages_NotPreview_Calls_AddCache_Correct_Key()
    {
        const string expectedKey = "IsSitemap_true";
        SetupResponse();
        var sut = GetService();
        await sut.GetCsPages(false);

        _cacheMock.Verify(o => o.AddToCache(expectedKey, It.IsAny<List<CsPage>>()), Times.Once);
    }

    [Fact]
    public async Task GetCsPages_Preview_Calls_AddCache_Correct_Key()
    {
        const string expectedKey = "IsSitemap_true";
        SetupResponse();
        var sut = GetService();
        await sut.GetCsPages();

        _cacheMock.Verify(o => o.AddToCache(expectedKey, It.IsAny<List<CsPage>>()), Times.Never);
    }

    [Fact]
    public async Task GetContent_Calls_Cache_Correct_Key()
    {
        const string slug = "dummy-slug";
        const string expectedKey = $"Slug_{slug}";
        SetupResponse();
        var sut = GetService();
        await sut.GetContent(slug, It.IsAny<bool>());

        _cacheMock.Verify(o => o.GetFromCache(expectedKey));
    }

    [Fact]
    public async Task GetCsPage_Calls_Cache_Correct_Key()
    {
        const string expectedKey = $"IsSitemap_true";
        SetupResponse();
        var sut = GetService();
        await sut.GetCsPages(It.IsAny<bool>());

        _cacheMock.Verify(o => o.GetFromCache(expectedKey));
    }

    [Fact]
    public async Task GetContentSupportPages_Calls_Cache_Correct_Key()
    {
        const string field = "field";
        const string value = "value";
        SetupResponse();
        var isPreview = It.IsAny<bool>();
        const string expectedKey = $"{field}_{value}";
        var sut = GetService();
        await sut.GetContentSupportPages(field, value, isPreview);

        _cacheMock.Verify(o => o.GetFromCache(expectedKey));
    }

    [Fact]
    public async Task GetContentSupportPages_GotCache_Returns_Cache()
    {
        var cacheValue = new List<CsPage> { It.IsAny<CsPage>() };

        const string field = "field";
        const string value = "value";
        const string expectedKey = $"{field}_{value}";
        var isPreview = It.IsAny<bool>();
        _cacheMock.Setup(o => o.GetFromCache(expectedKey)).Returns(cacheValue);

        var sut = GetService();
        var result = await sut.GetContentSupportPages(field, value, isPreview);

        result.Should().BeEquivalentTo(cacheValue);
    }

    [Fact]
    public async Task GetContentSupportPages_NotGotCache_Calls_Client()
    {
        List<CsPage>? cacheValue = null;

        const string field = "field";
        const string value = "value";
        const string expectedKey = $"{field}_{value}";
        SetupResponse();
        var isPreview = It.IsAny<bool>();
        _cacheMock.Setup(o => o.GetFromCache(expectedKey)).Returns(cacheValue);

        var sut = GetService();
        await sut.GetContentSupportPages(field, value, isPreview);

        _httpContentClientMock.Verify(o =>
                o.GetContentSupportPages(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetContentSupportPages_NotGotCache_AddsToCache()
    {
        List<CsPage>? cacheValue = null;

        const string field = "field";
        const string value = "value";
        const string expectedKey = $"{field}_{value}";
        SetupResponse();
        var isPreview = It.IsAny<bool>();
        _cacheMock.Setup(o => o.GetFromCache(expectedKey)).Returns(cacheValue);

        var sut = GetService();
        await sut.GetContentSupportPages(field, value, isPreview);

        _cacheMock.Verify(o => o.AddToCache(expectedKey, It.IsAny<List<CsPage>>()), Times.Once);
    }
}