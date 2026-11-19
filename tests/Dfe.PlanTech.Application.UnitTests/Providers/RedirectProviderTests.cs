using System.Reflection;
using Dfe.PlanTech.Application.Providers;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Providers;

public class RedirectProviderTests
{
    private readonly ILogger<RedirectProvider> _logger = Substitute.For<
        ILogger<RedirectProvider>
    >();
    private readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();

    private RedirectProvider CreateSut() => new(_logger, _contentfulService);

    // ---------------------------------------------------------------
    // Constructor
    // ---------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new RedirectProvider(null!, _contentfulService);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Constructor_NullContentfulService_ThrowsArgumentNullException()
    {
        var act = () => new RedirectProvider(_logger, null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    // ---------------------------------------------------------------
    // IsStaticPath
    // ---------------------------------------------------------------

    [Theory]
    [InlineData("api/foo")]
    [InlineData("API/FOO")]
    [InlineData("api")]
    public void IsStaticPath_PathStartsWithApi_ReturnsTrue(string path)
    {
        var sut = CreateSut();

        Assert.True(sut.IsStaticPath(path));
    }

    [Theory]
    [InlineData("healthy")]
    [InlineData("HEALTHY")]
    [InlineData("HeAlThY")]
    public void IsStaticPath_KnownDevPath_IsCaseInsensitiveMatch(string path)
    {
        var sut = CreateSut();

        Assert.True(sut.IsStaticPath(path));
    }

    [Fact]
    public void IsStaticPath_KnownDevPath_TrimsLeadingSlashBeforeComparison()
    {
        // "home" is registered internally as "/home" but
        // should match with no leading slash.
        var sut = CreateSut();

        Assert.True(sut.IsStaticPath(UrlConstants.Home));
    }

    [Fact]
    public void IsStaticPath_UnknownPath_ReturnsFalse()
    {
        var sut = CreateSut();

        Assert.False(sut.IsStaticPath("this-path-does-not-exist-anywhere"));
    }

    [Fact]
    public void IsStaticPath_UrlConstantsField_IsRecognisedAsKnown()
    {
        // The redirect provider uses UrlConstants fields to match against routes
        // which should not be redirected. Here we take the first constant starting
        // with '/' and assert IsStaticPath returns true.
        var constantPath = typeof(UrlConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .FirstOrDefault(val => val.StartsWith('/'));

        if (constantPath is null)
        {
            return; // No suitable constant found to assert against; nothing to verify.
        }

        var sut = CreateSut();
        var trimmedPath = constantPath.TrimStart('/');

        Assert.True(sut.IsStaticPath(trimmedPath));
    }

    // ---------------------------------------------------------------
    // TryGetRedirect - basic lookup
    // ---------------------------------------------------------------

    [Fact]
    public async Task TryGetRedirect_PathExists_ReturnsTarget()
    {
        _contentfulService.GetRedirectsAsync().Returns([new("old-page", "new-page")]);

        var sut = CreateSut();

        var result = await sut.TryGetRedirect("old-page");

        Assert.Equal("new-page", result);
    }

    [Fact]
    public async Task TryGetRedirect_PathDoesNotExist_ReturnsNull()
    {
        _contentfulService.GetRedirectsAsync().Returns([new("old-page", "new-page")]);

        var sut = CreateSut();

        var result = await sut.TryGetRedirect("some-other-page");

        Assert.Null(result);
    }

    [Fact]
    public async Task TryGetRedirect_LookupIsCaseInsensitive()
    {
        _contentfulService.GetRedirectsAsync().Returns([new("Old-page", "new-page")]);

        var sut = CreateSut();

        var result = await sut.TryGetRedirect("OLD-PAGE");

        Assert.Equal("new-page", result);
    }

    [Fact]
    public async Task TryGetRedirect_NoRedirectsConfigured_ReturnsNull()
    {
        _contentfulService.GetRedirectsAsync().Returns([]);

        var sut = CreateSut();

        var result = await sut.TryGetRedirect("anything");

        Assert.Null(result);
    }

    // ---------------------------------------------------------------
    // TryGetRedirect - chain flattening (A-B-C becomes A-C, B-C)
    // ---------------------------------------------------------------

    [Fact]
    public async Task TryGetRedirect_ChainOfRedirects_FlattensToFinalTarget()
    {
        _contentfulService.GetRedirectsAsync().Returns([new("a", "b"), new("b", "c")]);

        var sut = CreateSut();

        Assert.Equal("c", await sut.TryGetRedirect("a"));
        Assert.Equal("c", await sut.TryGetRedirect("b"));
    }

    [Fact]
    public async Task TryGetRedirect_LongerChain_FlattensAllHopsToFinalTarget()
    {
        _contentfulService
            .GetRedirectsAsync()
            .Returns([new("a", "b"), new("b", "c"), new("c", "d")]);

        var sut = CreateSut();

        Assert.Equal("d", await sut.TryGetRedirect("a"));
        Assert.Equal("d", await sut.TryGetRedirect("b"));
        Assert.Equal("d", await sut.TryGetRedirect("c"));
    }

    [Fact]
    public async Task TryGetRedirect_TargetNotItselfARedirectSource_ReturnsTargetUnchanged()
    {
        _contentfulService.GetRedirectsAsync().Returns([new("a", "b")]);

        var sut = CreateSut();

        Assert.Equal("b", await sut.TryGetRedirect("a"));
        Assert.Null(await sut.TryGetRedirect("b")); // "b" is a target, never a source
    }

    // ---------------------------------------------------------------
    // TryGetRedirect - duplicate "from" paths across entries
    // ---------------------------------------------------------------

    [Fact]
    public async Task TryGetRedirect_DuplicateFromPath_ExcludedFromResults_And_LogsWarning()
    {
        // Two separate redirect entries both claim "a" as a source with different targets.
        // BuildRedirectsAsync groups by from-path and only keeps groups with exactly one
        // entry, so ambiguous/duplicate sources should be dropped entirely.
        _contentfulService.GetRedirectsAsync().Returns([new("a", "b"), new("a", "c")]);

        var sut = CreateSut();

        Assert.Null(await sut.TryGetRedirect("a"));

        _logger
            .Received(1)
            .Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Duplicate redirects")),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    // ---------------------------------------------------------------
    // TryGetRedirect - error handling
    // ---------------------------------------------------------------

    [Fact]
    public async Task TryGetRedirect_ContentfulServiceThrows_ReturnsNull_And_LogsError()
    {
        _contentfulService
            .GetRedirectsAsync()
            .Returns<List<RedirectEntry>>(_ =>
                throw new InvalidOperationException("Contentful unavailable")
            );

        var sut = CreateSut();

        Assert.Null(await sut.TryGetRedirect("anything"));

        _logger
            .Received(1)
            .Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    // ---------------------------------------------------------------
    // TryGetRedirect - Lazy<Task<>> should only build once
    // ---------------------------------------------------------------

    [Fact]
    public async Task TryGetRedirect_CalledMultipleTimes_OnlyFetchesFromContentfulOnce()
    {
        _contentfulService.GetRedirectsAsync().Returns([new("a", "b")]);

        var sut = CreateSut();

        await sut.TryGetRedirect("a");
        await sut.TryGetRedirect("a");
        await sut.TryGetRedirect("nonexistent");

        await _contentfulService.Received(1).GetRedirectsAsync();
    }

    // ---------------------------------------------------------------
    // TryGetRedirect - circular redirects
    // ---------------------------------------------------------------

    [Fact]
    public async Task TryGetRedirect_CircularRedirect_LogsWarningAboutCircularPaths()
    {
        _contentfulService.GetRedirectsAsync().Returns([new("a", "b"), new("b", "a")]);

        var sut = CreateSut();
        await sut.TryGetRedirect("a");

        _logger
            .Received(1)
            .Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString()!.Contains("Circular redirects")),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
    }

    [Fact]
    public async Task TryGetRedirect_CircularRedirect_DoesNotThrowAndDoesNotHang()
    {
        _contentfulService.GetRedirectsAsync().Returns([new("a", "b"), new("b", "a")]);

        var sut = CreateSut();

        // Regression test:
        // This should complete rather than looping forever or throwing.
        await sut.TryGetRedirect("a");
        await sut.TryGetRedirect("b");
    }

    [Fact]
    public async Task TryGetRedirect_CircularRedirect_ReturnsNull()
    {
        _contentfulService.GetRedirectsAsync().Returns([new("a", "b"), new("b", "a")]);

        var sut = CreateSut();

        var resultForA = await sut.TryGetRedirect("a");

        Assert.Null(resultForA);
    }

    [Fact]
    public async Task TryGetRedirect_CircularRedirects_DoNotAffectGoodRoutes()
    {
        _contentfulService
            .GetRedirectsAsync()
            .Returns([new("a", "e"), new("c", "d"), new("d", "c"), new("e", "f")]);

        var sut = CreateSut();

        var resultForA = await sut.TryGetRedirect("a");

        Assert.Equal("f", resultForA);
    }
}
