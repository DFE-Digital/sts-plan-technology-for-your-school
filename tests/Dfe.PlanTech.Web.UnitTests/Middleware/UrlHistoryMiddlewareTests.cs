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
using NSubstitute;
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
        var requestMock = Substitute.For<HttpRequest>();

        requestMock.Scheme.Returns("https");
        requestMock.Host.Returns(new HostString("www.website.com"));
        requestMock.Path.Returns(new PathString("/three"));

        var contextMock = Substitute.For<HttpContext>();
        contextMock.Request.Returns(requestMock);

        var nextDelegateMock = Substitute.For<RequestDelegate>();

        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock);

        var history = new UrlHistory(_cacher);
        await urlHistory.InvokeAsync(contextMock, history);

        var historyCache = history.History;
        Assert.Equal(2, historyCache.Count);
        Assert.Equal(URL_SECOND, historyCache.Peek());
    }

    [Fact]
    public async Task Should_AddToHistory_When_Navigating_ToNewPage()
    {
        var requestMock = Substitute.For<HttpRequest>();
        requestMock.Scheme.Returns("https");
        requestMock.Host.Returns(new HostString(URL_FOURTH.Host));
        requestMock.Path.Returns(new PathString(URL_FOURTH.PathAndQuery));
        requestMock.Headers.Returns(new HeaderDictionary(){
            { UrlHistoryMiddleware.REFERER_HEADER_KEY, URL_FOURTH.ToString() }
        });

        var contextMock = Substitute.For<HttpContext>();
        contextMock.Request.Returns(requestMock);

        var nextDelegateMock = Substitute.For<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock);

        var history = new UrlHistory(_cacher);
        await urlHistory.InvokeAsync(contextMock, history);

        var historyCache = history.History;

        Assert.Equal(URL_FOURTH, historyCache.Peek());
        Assert.Equal(4, historyCache.Count);
    }

    [Fact]
    public async Task Should_Not_AddToHistory_When_Missing_Referer_Header()
    {
        var requestMock = Substitute.For<HttpRequest>();
        requestMock.Scheme.Returns("notarealscheme");
        requestMock.Host.Returns(new HostString("notarealhost"));
        requestMock.Path.Returns(new PathString("/"));
        requestMock.Headers.Returns(new HeaderDictionary());

        var contextMock = Substitute.For<HttpContext>();
        contextMock.Request.Returns(requestMock);

        var nextDelegateMock = Substitute.For<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock);

        var history = new UrlHistory(_cacher);
        await urlHistory.InvokeAsync(contextMock, history);

        var historyCache = history.History;

        Assert.Equal(3, historyCache.Count);
    }

    [Fact]
    public async Task Should_Not_AddToHistory_When_Error_Getting_Uri()
    {
        var requestMock = Substitute.For<HttpRequest>();
        requestMock.Scheme.Returns("");
        requestMock.Host.Returns(new HostString(""));
        requestMock.Path.Returns(new PathString("/"));
        requestMock.Headers.Returns(new HeaderDictionary(){
            { UrlHistoryMiddleware.REFERER_HEADER_KEY, URL_FOURTH.ToString() }
        });

        var contextMock = Substitute.For<HttpContext>();
        contextMock.Request.Returns(requestMock);

        var nextDelegateMock = Substitute.For<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock);

        var history = new UrlHistory(_cacher);
        await urlHistory.InvokeAsync(contextMock, history);

        var historyCache = history.History;

        Assert.Equal(3, historyCache.Count);
    }

    [Fact]
    public async Task Should_Log_Error_When_Uri_Parsing_Error()
    {
        var requestMock = Substitute.For<HttpRequest>();
        requestMock.Scheme.Returns("");
        requestMock.Host.Returns(new HostString(""));
        requestMock.Path.Returns(new PathString("/"));
        requestMock.Headers.Returns(new HeaderDictionary(){
            { UrlHistoryMiddleware.REFERER_HEADER_KEY, URL_FOURTH.ToString() }
        });

        var contextMock = Substitute.For<HttpContext>();
        contextMock.Request.Returns(requestMock);

        var nextDelegateMock = Substitute.For<RequestDelegate>();

        var loggerMock = Substitute.For<ILogger<UrlHistoryMiddleware>>();
        loggerMock.Log(Arg.Any<LogLevel>(),
                                                            Arg.Any<EventId>(),
                                                            It.IsAny<It.IsAnyType>(),
                                                            Arg.Any<Exception?>(),
                                                            Arg.Any<Func<It.IsAnyType, Exception?, string>>());

        var urlHistory = new UrlHistoryMiddleware(loggerMock, nextDelegateMock);

        var history = new UrlHistory(_cacher);
        await urlHistory.InvokeAsync(contextMock, history);

        var historyCache = history.History;

        loggerMock.Received().Log(Arg.Any<LogLevel>(),
                                                Arg.Any<EventId>(),
                                                Arg.Any<It.IsAnyType>(),
                                                Arg.Any<Exception?>(),
                                                Arg.Any<Func<It.IsAnyType, Exception?, string>>());
    }

}
