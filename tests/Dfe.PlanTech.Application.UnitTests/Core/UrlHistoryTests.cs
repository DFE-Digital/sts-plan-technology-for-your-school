using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Core;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Core;

public class UrlHistoryTests
{
    private Stack<string> history = new Stack<string>();

    private readonly ICacher cacher;

    public UrlHistoryTests()
    {
        var cacherMock = new Mock<ICacher>();
        cacherMock.Setup(cacher => cacher.Get<Stack<string>>(UrlHistory.CACHE_KEY)).Returns(history);

        cacherMock.Setup(cacher => cacher.Get<Stack<string>>(UrlHistory.CACHE_KEY, It.IsAny<Func<Stack<string>>>())).Returns(history);
        cacherMock.Setup(cacher => cacher.Set<Stack<string>>(UrlHistory.CACHE_KEY, It.IsAny<TimeSpan>(), It.IsAny<Stack<string>>()))
                .Callback((string key, TimeSpan timeSpan, Stack<string> stack) =>
                {
                    history = stack;
                });

        cacher = cacherMock.Object;
    }

    [Fact]
    public void Should_Add_Url_ToStack()
    {
        var url = "www.testurl.com";

        var urlHistory = new UrlHistory(cacher);
        urlHistory.AddUrlToHistory(url);

        var history = urlHistory.History;

        Assert.Equal(history.First(), url);
    }

    [Fact]
    public void Should_Remove_Url_FromStack()
    {
        var firstUrl = "www.first.com";
        var secondUrl = "www.second.com";

        var urlHistory = new UrlHistory(cacher);
        urlHistory.AddUrlToHistory(firstUrl);
        urlHistory.AddUrlToHistory(secondUrl);
        
        urlHistory.RemoveLastUrl();
        
        Assert.Equal(history.First(), firstUrl);
    }
}
