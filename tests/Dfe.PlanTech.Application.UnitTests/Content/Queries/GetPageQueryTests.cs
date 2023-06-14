using Dfe.PlanTech.Application.Caching.Interfaces;
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
    private readonly Mock<ISectionCacher> _sectionMock = new();

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
        _repoMock.Setup(client => client.GetEntities<Page>(It.IsAny<IGetEntitiesOptions>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((IGetEntitiesOptions options, CancellationToken token) =>
        {
            if (options.Queries != null)
            {
                foreach (var query in options.Queries)
                {
                    if (query is ContentQueryEquals equalsQuery && query.Field == "fields.slug")
                    {
                        return _mockData.Where(page => page.Slug == equalsQuery.Value);
                    }
                }
            }

            return Array.Empty<Page>();
        }).Verifiable();
    }

    [Fact]
    public async Task Should_Retrieve_Page_By_Slug()
    {
        var query = new GetPageQuery(_repoMock.Object, _sectionMock.Object);

        var result = await query.GetPageBySlug(LANDING_PAGE_SLUG);

        Assert.NotNull(result);

        _repoMock.VerifyAll();

        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);
    }

    [Fact]
    public async Task Should_ThrowException_When_SlugNotFound()
    {
        var query = new GetPageQuery(_repoMock.Object, _sectionMock.Object);

        await Assert.ThrowsAsync<Exception>(async () => await query.GetPageBySlug("NOT A REAL SLUG"));
    }
}
