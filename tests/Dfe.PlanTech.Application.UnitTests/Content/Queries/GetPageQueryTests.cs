using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetPageQueryTests
{
    private readonly Mock<IContentRepository> _repoMock = new();

    private List<Page> _mockData = new() {
        new Page(){
            Slug = "Index"
        },
        new Page(){
            Slug = "LandingPage"
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
            var pages = _mockData;

            return pages;
        }).Verifiable();
    }

    [Fact]
    public async Task Should_Retrieve_Page_By_Slug()
    {
        var query = new GetPageQuery(_repoMock.Object);

        var slug = "LandingPage";

        var result = await query.GetPageBySlug(slug);

        _repoMock.VerifyAll();
    }
}
