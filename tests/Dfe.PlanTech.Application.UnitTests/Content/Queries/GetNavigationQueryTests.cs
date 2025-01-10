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

    private readonly NavigationLink _contentfulLink = new NavigationLink
    {
        Href = "ContentfulHref",
        DisplayText = "ContentfulDisplayText"
    };

    private readonly IList<NavigationLink> _contentfulLinks;
    private readonly ILogger<GetNavigationQuery> _logger = Substitute.For<ILogger<GetNavigationQuery>>();

    public GetNavigationQueryTests()
    {
        _contentfulLinks = new List<NavigationLink> { _contentfulLink };
    }

    [Fact]
    public async Task Should_Retrieve_Nav_Links_From_Contentful()
    {
        _contentRepository.GetEntities<NavigationLink>(CancellationToken.None).Returns(_contentfulLinks);

        GetNavigationQuery navQuery = new(_logger, _contentRepository);

        var result = await navQuery.GetNavigationLinks();

        Assert.Equal(_contentfulLinks, result);
    }

    [Fact]
    public async Task Should_LogError_When_Contentful_Exception()
    {
        _contentRepository.GetEntities<NavigationLink>(CancellationToken.None).Throws(_ => new Exception("Contentful error"));

        GetNavigationQuery navQuery = new(_logger, _contentRepository);

        var result = await navQuery.GetNavigationLinks();

        var receivedLoggerMessages = _logger.GetMatchingReceivedMessages(GetNavigationQuery.ExceptionMessageContentful, LogLevel.Error);

        Assert.Single(receivedLoggerMessages);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Should_Retrieve_Nav_Link_By_Id_When_Exists()
    {
        _contentRepository.GetEntityById<NavigationLink>(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(_contentfulLink);
        var navQuery = new GetNavigationQuery(_logger, _contentRepository);
        var result = await navQuery.GetLinkById("contentId");

        Assert.NotNull(result);
        Assert.Equal(_contentfulLink.Href, result.Href);
        Assert.Equal(_contentfulLink.DisplayText, result.DisplayText);
    }

    [Fact]
    public async Task Should_Return_Null_When_Nav_Link_Does_Not_Exist()
    {
        _contentRepository.GetEntityById<NavigationLink?>(Arg.Any<string>(), Arg.Any<int>(), cancellationToken: CancellationToken.None).Returns((NavigationLink?)null);

        var navQuery = new GetNavigationQuery(_logger, _contentRepository);

        var result = await navQuery.GetLinkById("NonExistentId");

        Assert.Null(result);
    }
}
