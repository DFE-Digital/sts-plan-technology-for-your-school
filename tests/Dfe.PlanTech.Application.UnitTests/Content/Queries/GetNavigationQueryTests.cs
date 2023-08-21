using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetNavigationQueryTests
{
  private readonly IContentRepository _contentRepository;
  private IList<NavigationLink> _links = new List<NavigationLink>(){
    new NavigationLink(){
      Href = "Href",
      DisplayText = "DisplayText"
    }
  };

  public GetNavigationQueryTests()
  {
    _contentRepository = Substitute.For<IContentRepository>();
    _contentRepository.GetEntities<NavigationLink>(CancellationToken.None).Returns(_links);
  }

  [Fact]
  public async Task Should_Retrieve_Nav_Links()
  {
    IGetNavigationQuery navQuery = new GetNavigationQuery(_contentRepository);

    var result = await navQuery.GetNavigationLinks();

    Assert.Equal(_links, result);
  }
}