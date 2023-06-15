using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetPageQueryTests
{
    private const string SECTION_SLUG = "SectionSlugTest";
    private const string SECTION_TITLE = "SectionTitleTest";
    private const string LANDING_PAGE_SLUG = "LandingPage";
    private readonly Mock<IContentRepository> _repoMock = new();
    private readonly Mock<IQuestionnaireCacher> _questionnaireCacherMock = new Mock<IQuestionnaireCacher>();

    private readonly List<Page> _mockData = new() {
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
        }
    };

    public GetPageQueryTests()
    {
        SetupRepository();
        SetupQuestionnaireCacher();
    }

    private void SetupRepository()
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

    private void SetupQuestionnaireCacher()
    {
        _questionnaireCacherMock.Setup(questionnaire => questionnaire.Cached).Returns(new QuestionnaireCache()
        {
            CurrentSectionTitle = SECTION_TITLE
        });

        _questionnaireCacherMock.Setup(questionnaire => questionnaire.SaveCache(It.IsAny<QuestionnaireCache>()));
    }


    [Fact]
    public async Task Should_Retrieve_Page_By_Slug()
    {
        var query = new GetPageQuery(_questionnaireCacherMock.Object, _repoMock.Object);

        var result = await query.GetPageBySlug(LANDING_PAGE_SLUG);

        Assert.NotNull(result);

        _repoMock.VerifyAll();

        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);
    }

    [Fact]
    public async Task Should_ThrowException_When_SlugNotFound()
    {
        var query = new GetPageQuery(_questionnaireCacherMock.Object, _repoMock.Object);

        await Assert.ThrowsAsync<Exception>(async () => await query.GetPageBySlug("NOT A REAL SLUG"));
    }

    [Fact]
    public async Task Should_SetSectionTitle_When_DisplayTitle_IsTrue()
    {
        var query = new GetPageQuery(_questionnaireCacherMock.Object, _repoMock.Object);
        var result = await query.GetPageBySlug(SECTION_SLUG);


        Assert.Equal(SECTION_TITLE, result.SectionTitle);
        _questionnaireCacherMock.Verify(mock => mock.Cached, Times.Once);
    }
}
