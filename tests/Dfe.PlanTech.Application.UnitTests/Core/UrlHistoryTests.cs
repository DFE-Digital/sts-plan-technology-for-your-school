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
        cacherMock.Setup(cacher => cacher.Get<Stack<Uri>>(UrlHistory.CACHE_KEY)).Returns(history);

        cacherMock.Setup(cacher => cacher.Get(UrlHistory.CACHE_KEY, It.IsAny<Func<Stack<Uri>>>())).Returns(history);
        cacherMock.Setup(cacher => cacher.Set(UrlHistory.CACHE_KEY, It.IsAny<TimeSpan>(), It.IsAny<Stack<Uri>>()))
                .Callback((string key, TimeSpan timeSpan, Stack<Uri> stack) =>
                {
                    history = stack;
                });

        cacher = cacherMock.Object;
    }

    [Fact]
    public void Should_Add_Url_ToStack()
    {
        var url = new Uri("https://www.testurl.com");

        var urlHistory = new UrlHistory(cacher);
        urlHistory.AddUrlToHistory(url);

        var history = urlHistory.History;

        Assert.Equal(history.First(), url);
    }

    [Fact]
    public void Should_Remove_Url_FromStack()
    {
        var firstUrl = new Uri("https://www.first.com");
        var secondUrl = new Uri("https://www.second.com");

        var urlHistory = new UrlHistory(cacher);
        urlHistory.AddUrlToHistory(firstUrl);
        urlHistory.AddUrlToHistory(secondUrl);

        urlHistory.RemoveLastUrl();

        Assert.Equal(history.First(), firstUrl);
    }
}
