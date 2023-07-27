using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Caching.Models;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Core;

public class UrlHistoryTests
{
    private Stack<Uri> history = new();

    private readonly ICacher cacher;

    public UrlHistoryTests()
    {
        var cacherMock = new Mock<ICacher>();
        cacherMock.Setup(cacher => cacher.GetAsync<Stack<Uri>>(UrlHistory.CACHE_KEY)).ReturnsAsync(history);

        cacherMock.Setup(cacher => cacher.GetAsync(UrlHistory.CACHE_KEY, It.IsAny<Func<Stack<Uri>>>(), It.IsAny<TimeSpan?>())).ReturnsAsync(history);
        cacherMock.Setup(cacher => cacher.SetAsync(UrlHistory.CACHE_KEY, It.IsAny<Stack<Uri>>(), It.IsAny<TimeSpan?>()))
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

        var urlHistory = new UrlHistory(cacher);
        await urlHistory.AddUrlToHistory(url);

        var history = await urlHistory.History;

        Assert.Equal(history.First(), url);
    }

    [Fact]
    public async Task Should_Remove_Url_FromStack()
    {
        var firstUrl = new Uri("https://www.first.com");
        var secondUrl = new Uri("https://www.second.com");

        var urlHistory = new UrlHistory(cacher);
        await urlHistory.AddUrlToHistory(firstUrl);
        await urlHistory.AddUrlToHistory(secondUrl);

        await urlHistory.RemoveLastUrl();

        Assert.Equal(history.First(), firstUrl);
    }
}
