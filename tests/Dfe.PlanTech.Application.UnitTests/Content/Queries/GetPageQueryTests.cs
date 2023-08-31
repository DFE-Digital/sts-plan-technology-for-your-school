using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Models;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetPageQueryTests
{
    private const string SECTION_SLUG = "SectionSlugTest";
    private const string SECTION_TITLE = "SectionTitleTest";
    private const string LANDING_PAGE_SLUG = "LandingPage";
    private IContentRepository _repoSubstitute = Substitute.For<IContentRepository>();
    private IQuestionnaireCacher _questionnaireCacherSubstitute = Substitute.For<IQuestionnaireCacher>();

    private readonly List<Page> _Data = new() {
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

    public GetPageQueryTests()
    {
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
                        return _Data.Where(page => page.Slug == equalsQuery.Value);
                    }
                }
            }

            return Array.Empty<Page>();
        });
    }

    private void SetupQuestionnaireCacher()
    {
        _questionnaireCacherSubstitute.Cached.Returns(new QuestionnaireCache()
        {
            CurrentSectionTitle = SECTION_TITLE
        });

        _questionnaireCacherSubstitute.SaveCache(Arg.Any<QuestionnaireCache>());
    }


    [Fact]
    public async Task Should_Retrieve_Page_By_Slug()
    {
        var query = new GetPageQuery(_questionnaireCacherSubstitute, _repoSubstitute);

        var result = await query.GetPageBySlug(LANDING_PAGE_SLUG);

        Assert.NotNull(result);
        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);
    }

    [Fact]
    public async Task Should_ThrowException_When_SlugNotFound()
    {
        var query = new GetPageQuery(_questionnaireCacherSubstitute, _repoSubstitute);

        await Assert.ThrowsAsync<Exception>(async () => await query.GetPageBySlug("NOT A REAL SLUG"));
    }

    [Fact]
    public async Task Should_SetSectionTitle_When_DisplayTitle_IsTrue()
    {
        var query = new GetPageQuery(_questionnaireCacherSubstitute, _repoSubstitute);
        var result = await query.GetPageBySlug(SECTION_SLUG);


        Assert.Equal(SECTION_TITLE, result.SectionTitle);
        _ = _questionnaireCacherSubstitute.Received(1).Cached;
    }
}
