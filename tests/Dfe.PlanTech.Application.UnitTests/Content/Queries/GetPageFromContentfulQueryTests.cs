using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Options;
using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetPageFromContentfulQueryTests
{
    private const string TEST_PAGE_SLUG = "test-page-slug";
    private const string SECTION_SLUG = "SectionSlugTest";
    private const string LANDING_PAGE_SLUG = "LandingPage";

    private readonly IContentRepository _repoSubstitute = Substitute.For<IContentRepository>();
    private readonly ILogger<GetPageFromContentfulQuery> _logger = Substitute.For<ILogger<GetPageFromContentfulQuery>>();

    private readonly List<Page> _pages = new() {
        new Page(){
            Slug = "Index"
        },
        new Page(){
            Slug = LANDING_PAGE_SLUG,
        },
        new Page(){
            Slug = "AuditStart"
        },
        new Page(){
            Slug = SECTION_SLUG,
            DisplayTopicTitle = true,
            DisplayHomeButton= false,
            DisplayBackButton = false,
        },
        new Page(){
            Slug = TEST_PAGE_SLUG,
            Sys = new() {
                Id = "test-page-id"
            }
        },

    };


    public GetPageFromContentfulQueryTests()
    {
        SetupRepository();
    }

    private void SetupRepository()
    {
        _repoSubstitute.GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>())
        .Returns((callInfo) =>
        {
            IGetEntitiesOptions options = (IGetEntitiesOptions)callInfo[0];
            if (options.Queries != null)
            {
                foreach (var query in options.Queries)
                {
                    if (query is ContentQueryEquals equalsQuery && query.Field == "fields.slug")
                    {
                        return _pages.Where(page => page.Slug == equalsQuery.Value);
                    }
                }
            }

            return [];
        });
    }

    private GetPageFromContentfulQuery CreateGetPageQuery()
        => new(_repoSubstitute, _logger, new GetPageFromContentfulOptions() { Include = 4 });


    [Fact]
    public async Task Should_Retrieve_Page_By_Slug_From_Contentful()
    {
        var query = CreateGetPageQuery();

        var result = await query.GetPageBySlug(LANDING_PAGE_SLUG);

        Assert.NotNull(result);
        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(1).GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Return_Null_When_Slug_Not_Found()
    {
        var query = CreateGetPageQuery();
        var result = await query.GetPageBySlug("NOT A REAL SLUG");

        Assert.Null(result);
    }

    [Fact]
    public async Task Page_Not_Found_Exception_Is_Thrown_When_There_Is_An_Issue_Retrieving_Data()
    {
        _repoSubstitute.GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Test Exception"));

        var query = CreateGetPageQuery();

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(async () => await query.GetPageBySlug(SECTION_SLUG));
    }
}
