using System.Security.Claims;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Domain.SignIn.Enums;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Core;

public class UrlHistoryTests
{
    private Stack<Uri> history = new();

    private readonly ICacher cacher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UrlHistoryTests()
    {
        var usernameClaim = new Claim(UrlHistory.CLAIM_TYPE, "testing@test.com");

        var httpContextMock = new Mock<IHttpContextAccessor>();
        httpContextMock.Setup(httpContextMock => httpContextMock.HttpContext.User.Claims).Returns(() => new[] {
            usernameClaim
        });

        _httpContextAccessor = httpContextMock.Object;

        var cacherMock = new Mock<ICacher>();
        cacherMock.Setup(cacher => cacher.GetAsync<Stack<Uri>>(It.IsAny<string>())).ReturnsAsync(history);

        cacherMock.Setup(cacher => cacher.GetAsync(It.IsAny<string>(), It.IsAny<Func<Stack<Uri>>>(), It.IsAny<TimeSpan?>())).ReturnsAsync(history);
        cacherMock.Setup(cacher => cacher.SetAsync(It.IsAny<string>(), It.IsAny<Stack<Uri>>(), It.IsAny<TimeSpan?>()))
                .Callback((string key, Stack<Uri> stack, TimeSpan? timeSpan) =>
                {
                    history = stack;
                });

        cacher = cacherMock.Object;
    }

    [Fact]
    public async Task Should_Add_Url_ToStack()
    {
        var url = new Uri("https://www.testurl.com");

        var urlHistory = new UrlHistory(cacher, _httpContextAccessor);
        await urlHistory.AddUrlToHistory(url);

        var history = await urlHistory.History;

        Assert.Equal(history.First(), url);
    }

    [Fact]
    public async Task Should_Remove_Url_FromStack()
    {
        var firstUrl = new Uri("https://www.first.com");
        var secondUrl = new Uri("https://www.second.com");

        var urlHistory = new UrlHistory(cacher, _httpContextAccessor);
        await urlHistory.AddUrlToHistory(firstUrl);
        await urlHistory.AddUrlToHistory(secondUrl);

        await urlHistory.RemoveLastUrl();

        Assert.Equal(history.First(), firstUrl);
    }
}
