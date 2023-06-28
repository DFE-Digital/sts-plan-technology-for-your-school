using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class UrlHistoryMiddlewareTests
{
    private readonly Uri URL_FIRST = new("https://www.website.com/one");
    private readonly Uri URL_SECOND = new("https://www.website.com/two");
    private readonly Uri URL_THIRD = new("https://www.website.com/three");
    private readonly Uri URL_FOURTH = new("https://www.website.com/four");

    private readonly ICacher _cacher;

    public UrlHistoryMiddlewareTests()
    {
        _cacher = new Cacher(new CacheOptions(), new MemoryCache(new MemoryCacheOptions()));

        var history = new Stack<Uri>();
        history.Push(URL_FIRST);
        history.Push(URL_SECOND);
        history.Push(URL_THIRD);

        _cacher.Set(UrlHistory.CACHE_KEY, TimeSpan.FromHours(1), history);
    }

    [Fact]
    public async Task Should_PopHistory_When_Navigating_Backwards()
    {
        var requestMock = new Mock<HttpRequest>();

        requestMock.SetupGet(request => request.Scheme).Returns("https");
        requestMock.SetupGet(request => request.Host).Returns(new HostString("www.website.com"));
        requestMock.SetupGet(request => request.Path).Returns("/three");

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();

        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock.Object);

        var history = new UrlHistory(_cacher);
        await urlHistory.InvokeAsync(contextMock.Object, history);

        var historyCache = history.History;
        Assert.Equal(2, historyCache.Count);
        Assert.Equal(URL_SECOND, historyCache.Peek());
    }

    [Fact]
    public async Task Should_AddToHistory_When_Navigating_ToNewPage()
    {
        var requestMock = new Mock<HttpRequest>();
        requestMock.SetupGet(request => request.Scheme).Returns("https");
        requestMock.SetupGet(request => request.Host).Returns(new HostString(URL_FOURTH.Host));
        requestMock.SetupGet(request => request.Path).Returns(URL_FOURTH.PathAndQuery);
        requestMock.SetupGet(request => request.Headers).Returns(new HeaderDictionary(){
            { UrlHistoryMiddleware.REFERER_HEADER_KEY, URL_FOURTH.ToString() }
        });

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock.Object);

        var history = new UrlHistory(_cacher);
        await urlHistory.InvokeAsync(contextMock.Object, history);

        var historyCache = history.History;

        Assert.Equal(URL_FOURTH, historyCache.Peek());
        Assert.Equal(4, historyCache.Count);
    }

    [Fact]
    public async Task Should_Not_AddToHistory_When_Missing_Referer_Header()
    {
        var requestMock = new Mock<HttpRequest>();
        requestMock.SetupGet(request => request.Scheme).Returns("notarealscheme");
        requestMock.SetupGet(request => request.Host).Returns(new HostString("notarealhost"));
        requestMock.SetupGet(request => request.Path).Returns("/");
        requestMock.SetupGet(request => request.Headers).Returns(new HeaderDictionary());

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock.Object);

        var history = new UrlHistory(_cacher);
        await urlHistory.InvokeAsync(contextMock.Object, history);

        var historyCache = history.History;

        Assert.Equal(3, historyCache.Count);
    }

    [Fact]
    public async Task Should_Not_AddToHistory_When_Error_Getting_Uri()
    {
        var requestMock = new Mock<HttpRequest>();
        requestMock.SetupGet(request => request.Scheme).Returns(() => "");
        requestMock.SetupGet(request => request.Host).Returns(new HostString(""));
        requestMock.SetupGet(request => request.Path).Returns("/");
        requestMock.SetupGet(request => request.Headers).Returns(new HeaderDictionary(){
            { UrlHistoryMiddleware.REFERER_HEADER_KEY, URL_FOURTH.ToString() }
        });

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock.Object);

        var history = new UrlHistory(_cacher);
        await urlHistory.InvokeAsync(contextMock.Object, history);

        var historyCache = history.History;

        Assert.Equal(3, historyCache.Count);
    }

    [Fact]
    public async Task Should_Log_Error_When_Uri_Parsing_Error()
    {
        var requestMock = new Mock<HttpRequest>();
        requestMock.SetupGet(request => request.Scheme).Returns(() => "");
        requestMock.SetupGet(request => request.Host).Returns(new HostString(""));
        requestMock.SetupGet(request => request.Path).Returns("/");
        requestMock.SetupGet(request => request.Headers).Returns(new HeaderDictionary(){
            { UrlHistoryMiddleware.REFERER_HEADER_KEY, URL_FOURTH.ToString() }
        });

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();

        var loggerMock = new Mock<ILogger<UrlHistoryMiddleware>>();
        loggerMock.Setup(logger => logger.Log(It.IsAny<LogLevel>(),
                                                            It.IsAny<EventId>(),
                                                            It.IsAny<It.IsAnyType>(),
                                                            It.IsAny<Exception?>(),
                                                            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                    .Verifiable();

        var urlHistory = new UrlHistoryMiddleware(loggerMock.Object, nextDelegateMock.Object);

        var history = new UrlHistory(_cacher);
        await urlHistory.InvokeAsync(contextMock.Object, history);

        var historyCache = history.History;

        loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(),
                                                It.IsAny<EventId>(),
                                                It.IsAny<It.IsAnyType>(),
                                                It.IsAny<Exception?>(),
                                                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

}
