using AutoMapper;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetPageQueryTests
{
    private const string SECTION_SLUG = "SectionSlugTest";
    private const string SECTION_TITLE = "SectionTitleTest";
    private const string LANDING_PAGE_SLUG = "LandingPage";

    private readonly IContentRepository _repoSubstitute = Substitute.For<IContentRepository>();
    private readonly ICmsDbContext _cmsDbSubstitute = Substitute.For<ICmsDbContext>();
    private readonly ILogger<GetPageQuery> _logger = Substitute.For<ILogger<GetPageQuery>>();
    private readonly IMapper _mapperSubstitute = Substitute.For<IMapper>();
    private readonly IQuestionnaireCacher _questionnaireCacherSubstitute = Substitute.For<IQuestionnaireCacher>();

    private readonly List<Page> _pages = new() {
        new Page(){
            Slug = "Index"
        },
        new Page(){
            Slug = LANDING_PAGE_SLUG
        },
        new Page(){
            Slug = "AuditStart"
        },
        new Page(){
            Slug = SECTION_SLUG,
            DisplayTopicTitle = true,
            DisplayHomeButton= false,
            DisplayBackButton = false,
        }
    };

    private readonly List<PageDbEntity> _pagesFromDb = new() {
        new PageDbEntity(){
            Slug = "Index"
        },
        new PageDbEntity(){
            Slug = LANDING_PAGE_SLUG
        },
        new PageDbEntity(){
            Slug = "AuditStart"
        },
        new PageDbEntity(){
            Slug = SECTION_SLUG,
            DisplayTopicTitle = true,
            DisplayHomeButton= false,
            DisplayBackButton = false,
        }
    };


    public GetPageQueryTests()
    {
        _cmsDbSubstitute.ToListAsync(Arg.Any<IQueryable<PageDbEntity>>())
                        .Returns(callinfo =>
                        {
                            var queryable = callinfo.ArgAt<IQueryable<PageDbEntity>>(0);

                            return queryable.ToList();
                        });

        _mapperSubstitute.Map<Page>(Arg.Any<PageDbEntity>())
                        .Returns(callinfo =>
                        {
                            var page = callinfo.ArgAt<PageDbEntity>(0);

                            return new Page()
                            {
                                Slug = page.Slug,
                                DisplayBackButton = page.DisplayBackButton,
                                DisplayHomeButton = page.DisplayBackButton,
                                DisplayOrganisationName = page.DisplayOrganisationName,
                                DisplayTopicTitle = page.DisplayTopicTitle
                            };
                        });

        SetupRepository();
        SetupQuestionnaireCacher();
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

            return Array.Empty<Page>();
        });
    }

    private GetPageQuery CreateGetPageQuery()
        => new(_cmsDbSubstitute, _logger, _mapperSubstitute, _questionnaireCacherSubstitute, _repoSubstitute);

    private void SetupQuestionnaireCacher()
    {
        _questionnaireCacherSubstitute.Cached.Returns(new QuestionnaireCache()
        {
            CurrentSectionTitle = SECTION_TITLE
        });

        _questionnaireCacherSubstitute.SaveCache(Arg.Any<QuestionnaireCache>());
    }


    [Fact]
    public async Task Should_Retrieve_Page_By_Slug_From_Db()
    {
        GetPageQuery query = CreateGetPageQuery();

        _cmsDbSubstitute.Pages.Returns(_pagesFromDb.AsQueryable());

        var result = await query.GetPageBySlug(LANDING_PAGE_SLUG);

        Assert.NotNull(result);
        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(0).GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).ToListAsync(Arg.Any<IQueryable<PageDbEntity>>());
    }

    [Fact]
    public async Task Should_Retrieve_Page_By_Slug_From_Contentful_When_Db_Page_Not_Found()
    {
        GetPageQuery query = CreateGetPageQuery();

        var emptyList = new List<PageDbEntity>(0);
        _cmsDbSubstitute.Pages.Returns(emptyList.AsQueryable());

        var result = await query.GetPageBySlug(LANDING_PAGE_SLUG);

        Assert.NotNull(result);
        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(1).GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.Received(1).ToListAsync(Arg.Any<IQueryable<PageDbEntity>>());
    }

    [Fact]
    public async Task Should_ThrowException_When_SlugNotFound()
    {
        var query = CreateGetPageQuery();

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(async () => await query.GetPageBySlug("NOT A REAL SLUG"));
    }

    [Fact]
    public async Task Should_SetSectionTitle_When_DisplayTitle_IsTrue()
    {
        var query = CreateGetPageQuery();
        var result = await query.GetPageBySlug(SECTION_SLUG);

        Assert.Equal(SECTION_TITLE, result.SectionTitle);
        _ = _questionnaireCacherSubstitute.Received(1).Cached;
    }

    [Fact]
    public async Task ContentfulDataUnavailable_Exception_Is_Thrown_When_There_Is_An_Issue_Retrieving_Data()
    {
        _repoSubstitute.GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Test Exception"));

        var query = CreateGetPageQuery();

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(async () => await query.GetPageBySlug(SECTION_SLUG));
    }

    [Fact]
    public async Task ContentfulDataUnavailable_Exception_Is_Thrown_When_There_Is_An_Issue_Incorrect_Env_Variable_Passed()
    {
        Environment.SetEnvironmentVariable("CONTENTFUL_GET_ENTITY_INT", "FOUR");

        _repoSubstitute.GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Test Exception"));

        var query = CreateGetPageQuery();

        await Assert.ThrowsAsync<ContentfulDataUnavailableException>(async () => await query.GetPageBySlug(SECTION_SLUG));
    }

}
