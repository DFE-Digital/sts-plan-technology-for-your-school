using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Application.Converters;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using System.Text.Json;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Middleware;

public class UrlHistoryMiddlewareTests
{
    private readonly Uri URL_FIRST = new("https://www.website.com/one");
    private readonly Uri URL_SECOND = new("https://www.website.com/two");
    private readonly Uri URL_THIRD = new("https://www.website.com/three");
    private readonly Uri URL_FOURTH = new("https://www.website.com/four");

    private readonly ICacher _cacher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Mock<HttpContext> _httpContextMock = new();

    private readonly IDistributedCache _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

    public UrlHistoryMiddlewareTests()
    {
        var userEmail = "test@testing.com";

        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new JsonConverterFactoryForStackOfT());
        _cacher = new Cacher(_cache, jsonOptions);

        var history = new Stack<Uri>();
        history.Push(URL_FIRST);
        history.Push(URL_SECOND);
        history.Push(URL_THIRD);

        _cacher.SetAsync(UrlHistory.CACHE_KEY + ":" + userEmail, history);

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        httpContextAccessorMock.Setup(httpContextMock => httpContextMock.HttpContext).Returns(() => _httpContextMock.Object);
        _httpContextMock.Setup(httpContextMock => httpContextMock.User).Returns(() =>
        {
            var claims = new[] { new Claim(BaseCacher.USER_CLAIM, "test@testing.com") };
            var claimsIdentity = new ClaimsIdentity(claims, "Any");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return claimsPrincipal;
        });

        _httpContextAccessor = httpContextAccessorMock.Object;

    }

    [Fact]
    public async Task Should_PopHistory_When_Navigating_Backwards()
    {
        Mock<HttpRequest> requestMock = SetupHttpRequestMock("https", "www.website.com", "/three", URL_FOURTH.ToString());

        _httpContextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();

        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock.Object);

        var history = new UrlHistory(_cacher, _httpContextAccessor);
        await urlHistory.InvokeAsync(_httpContextMock.Object, history);

        var historyCache = await history.History;

        Assert.Equal(2, historyCache.Count);
        Assert.Equal(URL_SECOND, historyCache.Peek());
    }

    [Fact]
    public async Task Should_AddToHistory_When_Navigating_ToNewPage()
    {
        var requestMock = SetupHttpRequestMock("https", URL_FOURTH.Host, URL_FOURTH.PathAndQuery, URL_FOURTH.ToString());

        _httpContextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock.Object);

        var history = new UrlHistory(_cacher, _httpContextAccessor);
        await urlHistory.InvokeAsync(_httpContextMock.Object, history);

        var historyCache = await history.History;

        Assert.Equal(URL_FOURTH, historyCache.Peek());
        Assert.Equal(4, historyCache.Count);
    }

    [Fact]
    public async Task Should_Not_AddToHistory_When_Missing_Referer_Header()
    {
        var requestMock = SetupHttpRequestMock("notarealscheme", "notarealhost", "/", null);

        _httpContextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock.Object);

        var history = new UrlHistory(_cacher, _httpContextAccessor);
        await urlHistory.InvokeAsync(_httpContextMock.Object, history);

        var historyCache = await history.History;

        Assert.Equal(3, historyCache.Count);
    }

    [Fact]
    public async Task Should_Not_AddToHistory_When_Error_Getting_Uri()
    {
        var requestMock = SetupHttpRequestMock("", "", "/", URL_FOURTH.ToString());

        _httpContextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();
        var urlHistory = new UrlHistoryMiddleware(new NullLogger<UrlHistoryMiddleware>(), nextDelegateMock.Object);

        var history = new UrlHistory(_cacher, _httpContextAccessor);
        await urlHistory.InvokeAsync(_httpContextMock.Object, history);

        var historyCache = await history.History;

        Assert.Equal(3, historyCache.Count);
    }

    [Fact]
    public async Task Should_Log_Error_When_Uri_Parsing_Error()
    {
        var requestMock = SetupHttpRequestMock("", "", "/", URL_FOURTH.ToString());

        _httpContextMock.Setup(context => context.Request).Returns(() => requestMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();

        var loggerMock = new Mock<ILogger<UrlHistoryMiddleware>>();
        loggerMock.Setup(logger => logger.Log(It.IsAny<LogLevel>(),
                                                            It.IsAny<EventId>(),
                                                            It.IsAny<It.IsAnyType>(),
                                                            It.IsAny<Exception?>(),
                                                            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                    .Verifiable();

        var urlHistory = new UrlHistoryMiddleware(loggerMock.Object, nextDelegateMock.Object);

        var history = new UrlHistory(_cacher, _httpContextAccessor);
        await urlHistory.InvokeAsync(_httpContextMock.Object, history);

        var historyCache = history.History;

        loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(),
                                                It.IsAny<EventId>(),
                                                It.IsAny<It.IsAnyType>(),
                                                It.IsAny<Exception?>(),
                                                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }


    private static Mock<HttpRequest> SetupHttpRequestMock(string scheme, string host, string path, string? referer)
    {
        var requestMock = new Mock<HttpRequest>();

        requestMock.SetupGet(request => request.Scheme).Returns(scheme);
        requestMock.SetupGet(request => request.Host).Returns(new HostString(host));
        requestMock.SetupGet(request => request.Path).Returns(path);
        requestMock.SetupGet(request => request.Headers).Returns(referer != null ? new HeaderDictionary(){
            { UrlHistoryMiddleware.REFERER_HEADER_KEY, referer}
        } : new HeaderDictionary());
        return requestMock;
    }
}