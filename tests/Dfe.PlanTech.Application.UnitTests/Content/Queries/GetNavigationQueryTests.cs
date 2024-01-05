using System.Dynamic;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetNavigationQueryTests
{
  private readonly IContentRepository _contentRepository;

  private readonly IList<NavigationLink> _links = new List<NavigationLink>(){
    new(){
      Href = "Href",
      DisplayText = "DisplayText"
    }
  };

  private readonly ILogger<GetNavigationQuery> _logger = new NullLogger<GetNavigationQuery>();
  private readonly ICmsDbContext _db = Substitute.For<ICmsDbContext>();

  public GetNavigationQueryTests()
  {
    _contentRepository = Substitute.For<IContentRepository>();
    _contentRepository.GetEntities<NavigationLink>(CancellationToken.None).Returns(_links);

    _db.NavigationLink.Returns(_links.Select(link => new NavigationLinkDbEntity
    {
      Href = link.Href,
      DisplayText = link.DisplayText
    }).AsQueryable());

    _db.ToListAsync(Arg.Any<IQueryable<NavigationLinkDbEntity>>()).Returns(callInfo =>
    {
      var queryable = callInfo.ArgAt<IQueryable<NavigationLinkDbEntity>>(1);

      return Task.FromResult(queryable.ToList());
    });
  }

  [Fact]
  public async Task Should_Retrieve_Nav_Links()
  {
    IGetNavigationQuery navQuery = new GetNavigationQuery(_db, _logger, _contentRepository);

    var result = await navQuery.GetNavigationLinks();

    Assert.Equal(_links, result);
  }
}