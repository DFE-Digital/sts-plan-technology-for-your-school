using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetPageQueryTests
{
    private const string LANDING_PAGE_SLUG = "LandingPage";
    private readonly Mock<IContentRepository> _repoMock = new();

    private readonly List<Page> _mockData = new() {
        new Page(){
            Slug = "Index"
        },
        new Page(){
            Slug = LANDING_PAGE_SLUG
        },
        new Page(){
            Slug = "AuditStart"
        }
    };

    public GetPageQueryTests()
    {
        _repoMock.Setup(client => client.GetEntities<Page>(It.IsAny<IEnumerable<IContentQuery>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((IEnumerable<IContentQuery> queries, CancellationToken token) =>
        {
            foreach (var query in queries)
            {
                if (query is ContentQueryEquals equalsQuery && query.Field == "fields.slug")
                {
                    return _mockData.Where(page => page.Slug == equalsQuery.Value);
                }
            }

            return Array.Empty<Page>();
        }).Verifiable();
    }

    [Fact]
    public async Task Should_Retrieve_Page_By_Slug()
    {
        var query = new GetPageQuery(_repoMock.Object);

        var result = await query.GetPageBySlug(LANDING_PAGE_SLUG);

        Assert.NotNull(result);

        _repoMock.VerifyAll();

        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);
    }

    [Fact]
    public async Task Should_ThrowException_When_SlugNotFound()
    {
        var query = new GetPageQuery(_repoMock.Object);

        await Assert.ThrowsAsync<Exception>(async () => await query.GetPageBySlug("NOT A REAL SLUG"));
    }
}
