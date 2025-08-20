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

public class GetPageQueryTests
{
    private const string TEST_PAGE_SLUG = "test-page-slug";
    private const string SECTION_SLUG = "SectionSlugTest";
    private const string LANDING_PAGE_SLUG = "LandingPage";
    private const string LANDING_PAGE_ID = "LandingPageId";

    private readonly IContentRepository _repoSubstitute = Substitute.For<IContentRepository>();
    private readonly ILogger<GetPageQuery> _logger = Substitute.For<ILogger<GetPageQuery>>();

    private readonly List<Page> _pages = new() {
        new Page(){
            Sys = new SystemDetails { Id = "index-page-id" },
            Slug = "Index"
        },
        new Page(){
            Sys = new SystemDetails { Id = LANDING_PAGE_ID },
            Slug = LANDING_PAGE_SLUG,
        },
        new Page(){
            Sys = new SystemDetails { Id = "audit-page-id" },
            Slug = "AuditStart"
        },
        new Page(){
            Sys = new SystemDetails { Id = "section-page-id" },
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


    public GetPageQueryTests()
    {
        SetupRepository();
    }

    private void SetupRepository()
    {
        _repoSubstitute.GetEntities<Page>(Arg.Any<IGetEntriesOptions>(), Arg.Any<CancellationToken>())
        .Returns((callInfo) =>
        {
            IGetEntriesOptions options = (IGetEntriesOptions)callInfo[0];
            if (options.Queries != null)
            {
                foreach (var query in options.Queries)
                {
                    if (query is ContentQuerySingleValue equalsQuery && query.Field == "fields.slug")
                    {
                        return _pages.Where(page => page.Slug == equalsQuery.Value);
                    }
                }
            }

            return [];
        });
        _repoSubstitute.GetEntityById<Page>(Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns((callInfo) =>
            {
                var pageId = (string)callInfo[0];
                return _pages.FirstOrDefault(page => page.Sys.Id == pageId);
            });
    }

    private GetPageQuery CreateGetPageQuery()
        => new(_repoSubstitute, _logger, new GetPageFromContentfulOptions() { Include = 4 });


    [Fact]
    public async Task Should_Retrieve_Page_By_Slug_From_Contentful()
    {
        var query = CreateGetPageQuery();

        var result = await query.GetPageBySlug(LANDING_PAGE_SLUG);

        Assert.NotNull(result);
        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(1).GetEntities<Page>(Arg.Any<IGetEntriesOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Retrieve_Page_By_Id_From_Contentful()
    {
        var query = CreateGetPageQuery();

        var result = await query.GetPageById(LANDING_PAGE_ID);

        Assert.NotNull(result);
        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(1).GetEntityById<Page>(Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>());
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
        _repoSubstitute.GetEntities<Page>(Arg.Any<IGetEntriesOptions>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Test Exception"));

        var query = CreateGetPageQuery();

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(async () => await query.GetPageBySlug(SECTION_SLUG));
    }
}
