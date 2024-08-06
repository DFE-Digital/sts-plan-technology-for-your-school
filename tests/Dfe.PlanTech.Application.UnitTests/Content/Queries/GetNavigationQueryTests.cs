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

    private readonly IList<NavigationLink> _contentfulLinks = new List<NavigationLink>(){
    new(){
      Href = "ContentfulHref",
      DisplayText = "ContentfulDisplayText"
    }
  };

    private readonly IList<NavigationLinkDbEntity> _dbLinks = new List<NavigationLinkDbEntity>(){
    new(){
      Href = "DatabaseHref",
      DisplayText = "DatabaseLink"
    }
  };

    private readonly ILogger<GetNavigationQuery> _logger = Substitute.For<ILogger<GetNavigationQuery>>();

    private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();

    public GetNavigationQueryTests()
    {
    }

    [Fact]
    public async Task Should_Retrieve_Nav_Links_From_Database()
    {
        _db.NavigationLink.Returns(_dbLinks.AsQueryable());
        _db.ToListAsync(Arg.Any<IQueryable<NavigationLinkDbEntity>>()).Returns(callInfo =>
        {
            var queryable = callInfo.ArgAt<IQueryable<NavigationLinkDbEntity>>(0);

            return queryable.ToList();
        });

        GetNavigationQuery navQuery = new GetNavigationQuery(_db, _logger, _contentRepository);

        var result = await navQuery.GetNavigationLinks();

        Assert.Equal(_dbLinks, result);
    }

    [Fact]
    public async Task Should_Retrieve_Nav_Links_From_Contentful_When_No_Db_Results()
    {
        _contentRepository.GetEntities<NavigationLink>(CancellationToken.None).Returns(_contentfulLinks);

        var emptyNavLinksList = new List<NavigationLinkDbEntity>();
        _db.NavigationLink.Returns(emptyNavLinksList.AsQueryable());
        _db.ToListAsync(Arg.Any<IQueryable<NavigationLinkDbEntity>>()).Returns(callInfo =>
        {
            var queryable = callInfo.ArgAt<IQueryable<NavigationLinkDbEntity>>(0);

            return queryable.ToList();
        });

        GetNavigationQuery navQuery = new GetNavigationQuery(_db, _logger, _contentRepository);

        var result = await navQuery.GetNavigationLinks();

        Assert.Equal(_contentfulLinks, result);
    }

    [Fact]
    public async Task Should_LogError_When_DbException()
    {
        _contentRepository.GetEntities<NavigationLink>(CancellationToken.None).Returns(_contentfulLinks);

        var emptyNavLinksList = new List<NavigationLinkDbEntity>();
        _db.NavigationLink.Returns(emptyNavLinksList.AsQueryable());

        _db.ToListAsync(Arg.Any<IQueryable<NavigationLinkDbEntity>>()).Throws(callInfo =>
        {
            throw new Exception("Error occurred");
        });

        GetNavigationQuery navQuery = new GetNavigationQuery(_db, _logger, _contentRepository);

        var result = await navQuery.GetNavigationLinks();

        _logger.ReceivedWithAnyArgs(1).Log(default, default, default, default, default!);
        Assert.Equal(_contentfulLinks, result);
    }

    [Fact]
    public async Task Should_LogError_When_Contentful_Exception()
    {
        var emptyNavLinksList = new List<NavigationLinkDbEntity>();
        _db.NavigationLink.Returns(emptyNavLinksList.AsQueryable());

        _db.ToListAsync(Arg.Any<IQueryable<NavigationLinkDbEntity>>()).Returns(callInfo =>
        {
            var queryable = callInfo.ArgAt<IQueryable<NavigationLinkDbEntity>>(0);

            return queryable.ToList();
        });

        _contentRepository.GetEntities<NavigationLink>(CancellationToken.None).Throws(callinfo =>
        {
            throw new Exception("Contentful error");
        });


        GetNavigationQuery navQuery = new GetNavigationQuery(_db, _logger, _contentRepository);

        var result = await navQuery.GetNavigationLinks();

        _logger.ReceivedWithAnyArgs(1).Log(default, default, default, default, default!);
        Assert.Empty(result);
    }

}
