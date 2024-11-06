using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetNavigationQueryTests
{
    private readonly IContentRepository _contentRepository = Substitute.For<IContentRepository>();
    private readonly ICmsCache _cache = Substitute.For<ICmsCache>();

    private readonly IList<NavigationLink> _contentfulLinks = new List<NavigationLink>()
    {
        new()
        {
            Href = "ContentfulHref",
            DisplayText = "ContentfulDisplayText"
        }
    };

    private readonly ILogger<GetNavigationQuery> _logger = Substitute.For<ILogger<GetNavigationQuery>>();

    public GetNavigationQueryTests()
    {
        _cache.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Func<Task<IEnumerable<NavigationLink>>>>())
            .Returns(callInfo =>
            {
                var func = callInfo.ArgAt<Func<Task<IEnumerable<NavigationLink>>>>(1);
                return func();
            });
    }

    [Fact]
    public async Task Should_Retrieve_Nav_Links_From_Contentful()
    {
        _contentRepository.GetEntities<NavigationLink>(CancellationToken.None).Returns(_contentfulLinks);

        GetNavigationQuery navQuery = new(_logger, _contentRepository, _cache);

        var result = await navQuery.GetNavigationLinks();

        Assert.Equal(_contentfulLinks, result);
    }

    [Fact]
    public async Task Should_LogError_When_Contentful_Exception()
    {
        _contentRepository.GetEntities<NavigationLink>(CancellationToken.None).Throws(_ => new Exception("Contentful error"));

        GetNavigationQuery navQuery = new(_logger, _contentRepository, _cache);

        var result = await navQuery.GetNavigationLinks();

        var receivedLoggerMessages = _logger.GetMatchingReceivedMessages(GetNavigationQuery.ExceptionMessageContentful, LogLevel.Error);

        Assert.Single(receivedLoggerMessages);
        Assert.Empty(result);
    }

}
