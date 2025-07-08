using Dfe.PlanTech.Application.Caching.Models;
using Dfe.PlanTech.Core.Caching.Interfaces;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Core;

public class UrlHistoryTests
{
    private Stack<Uri> history = new();

    private readonly ICacher cacher;

    public UrlHistoryTests()
    {
        var cacherSubstitute = Substitute.For<ICacher>();
        cacherSubstitute.Get<Stack<Uri>>(UrlHistory.CACHE_KEY).Returns(history);

        cacherSubstitute.Get(UrlHistory.CACHE_KEY, Arg.Any<Func<Stack<Uri>>>()).Returns(history);
        cacherSubstitute.When(x => x.Set(UrlHistory.CACHE_KEY, Arg.Any<TimeSpan>(), Arg.Any<Stack<Uri>>()))
            .Do((callInfo) =>
            {
                history = (Stack<Uri>)callInfo[2];
            });

        cacher = cacherSubstitute;
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
