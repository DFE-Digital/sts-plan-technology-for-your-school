using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class UrlHistoryMiddlewareTests
{
    private const string URL_FIRST = "www.website.com/one";
    private const string URL_SECOND = "www.website.com/two";
    private const string URL_THIRD = "www.website.com/three";

    private readonly ICacher _cacher;

    public UrlHistoryMiddlewareTests()
    {
        _cacher = new Cacher(new CacheOptions(), new MemoryCache(new MemoryCacheOptions()));

        var history = new Stack<string>();
        history.Push(URL_FIRST);
        history.Push(URL_SECOND);
        history.Push(URL_THIRD);

        _cacher.Set(UrlHistoryMiddleware.CACHE_KEY, TimeSpan.FromHours(1), history);
    }

    [Fact]
    public void Should_PopHistory_When_Navigating_Backwards()
    {
        var requestMock = new Mock<HttpRequest>();
        requestMock.SetupGet(request => request.Host).Returns(new HostString("www.website.com"));
        requestMock.SetupGet(request => request.Path).Returns("/three");

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(nextDelegateMock.Object);

        urlHistory.InvokeAsync(contextMock.Object, _cacher);

        var historyCache = _cacher.Get<Stack<string>>(UrlHistoryMiddleware.CACHE_KEY)!;
        Assert.Equal(2, historyCache.Count);
        Assert.Equal(URL_SECOND, historyCache.Peek());
    }

    [Fact]
    public void Should_AddToHistory_When_Navigating_ToNewPage()
    {
        var requestMock = new Mock<HttpRequest>();
        requestMock.SetupGet(request => request.Host).Returns(new HostString("www.website.com"));
        requestMock.SetupGet(request => request.Path).Returns("/five");
        requestMock.SetupGet(request => request.Headers).Returns(new HeaderDictionary(){
            { "Referer", "www.website.com/four" }
        });

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(nextDelegateMock.Object);

        urlHistory.InvokeAsync(contextMock.Object, _cacher);

        var historyCache = _cacher.Get<Stack<string>>(UrlHistoryMiddleware.CACHE_KEY)!;

        Assert.Equal(4, historyCache.Count);
        Assert.Equal("www.website.com/four", historyCache.Peek());
    }
}
