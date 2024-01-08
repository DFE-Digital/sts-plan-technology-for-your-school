using AutoMapper;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Exceptions;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Application.UnitTests.Content.Queries;

public class GetPageQueryTests
{
    private const string TEST_PAGE_SLUG = "test-page-slug";
    private const string SECTION_SLUG = "SectionSlugTest";
    private const string SECTION_TITLE = "SectionTitleTest";
    private const string LANDING_PAGE_SLUG = "LandingPage";
    private const string BUTTON_REF_SLUG = "ButtonReferences";
    private const string CATEGORY_ID = "category-one";
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

    private readonly List<PageDbEntity> _pagesFromDb = new() {
        new PageDbEntity(){
            Slug = "Index",
            Content = new(){
                new HeaderDbEntity()
            }
        },
        new PageDbEntity(){
            Slug = TEST_PAGE_SLUG,
            Id = "test-page-id"
        },
        new PageDbEntity(){
            Slug = LANDING_PAGE_SLUG,
            Content = new(){
                new HeaderDbEntity(),
            }
        },
        new PageDbEntity(){
            Slug = "AuditStart",
        },
        new PageDbEntity()
    {
        Slug = SECTION_SLUG,
            DisplayTopicTitle = true,
            DisplayHomeButton = false,
            DisplayBackButton = false,
        },
        _pageWithButton,
        _pageWithCategories,
        _pageWithRichTextContent,
    };

    private readonly static PageDbEntity _pageWithCategories = new()
    {
        Slug = "categories_page",
        Id = CATEGORY_ID,
        Content = new()
        {
        }
    };

    private readonly static CategoryDbEntity _category = new()
    {
        Id = CATEGORY_ID,
        ContentPages = new()
        {

        },
        Sections = new()
        {

        }
    };

    private readonly static List<SectionDbEntity> _sections = new(){
                new SectionDbEntity(){
                    Name = "sSection one",
                    CategoryId = CATEGORY_ID,
                    Recommendations = new(){
                        new RecommendationPageDbEntity(){
                            DisplayName = "Recommendation one",
                            Maturity = Maturity.High,
                            Page = new(){
                                Slug = "recommendation-high"
                            }
                        },
                        new RecommendationPageDbEntity(){
                            DisplayName = "Recommendation two",
                            Maturity = Maturity.Medium,
                            Page = new(){
                                Slug = "recommendation-medium"
                            }
                        }
                    },
                    Questions = new(){
                        new QuestionDbEntity(){
                            Slug = "question-one-slug",
                        },
                        new QuestionDbEntity(){
                            Slug = "question-two-slug"
                        }
                    }
                },
            };

    private readonly static PageDbEntity _pageWithButton = new()
    {
        Slug = BUTTON_REF_SLUG,
        Content = new(){
                new ButtonWithEntryReferenceDbEntity(){
                    Button = new ButtonDbEntity(){
                        Value = "Button value",
                        IsStartButton = true
                    }
                }
            }
    };

    public static PageDbEntity _pageWithRichTextContent = new()
    {
        Slug = "rich_text_content",
        Content = new()
        {
            new TextBodyDbEntity(){
                RichTextId = 1
            },
            new TextBodyDbEntity(){
                RichTextId = 2
            },
        }
    };

    public GetPageQueryTests()
    {
        _cmsDbSubstitute.ToListAsync(Arg.Any<IQueryable<SectionDbEntity>>())
                        .Returns(callinfo =>
                        {
                            var queryable = callinfo.ArgAt<IQueryable<SectionDbEntity>>(0);

                            return queryable.ToList();
                        });

        _mapperSubstitute.Map<PageDbEntity, Page>(Arg.Any<PageDbEntity>())
                        .Returns(callinfo =>
                        {
                            var page = callinfo.ArgAt<PageDbEntity>(0);

                            return new Page()
                            {
                                Slug = page.Slug,
                                DisplayBackButton = page.DisplayBackButton,
                                DisplayHomeButton = page.DisplayBackButton,
                                DisplayOrganisationName = page.DisplayOrganisationName,
                                DisplayTopicTitle = page.DisplayTopicTitle,
                                Content = page.Content.Select(content =>
                                {
                                    if (content is CategoryDbEntity category)
                                    {
                                        return new Category()
                                        {
                                            Sys = new()
                                            {
                                                Id = category.Id,
                                            },
                                            Sections = category.Sections.Select(section => new Section()
                                            {
                                                Sys = new()
                                                {
                                                    Id = section.Id
                                                },
                                                Recommendations = section.Recommendations.Select(recommendation => new RecommendationPage()
                                                {
                                                    Sys = new()
                                                    {
                                                        Id = recommendation.Id
                                                    }
                                                }).ToList(),
                                                Questions = section.Questions.Select(question => new Question()
                                                {
                                                    Sys = new()
                                                    {
                                                        Id = question.Id
                                                    }
                                                }).ToList(),
                                            }).ToList()
                                        } as ContentComponent;
                                    }

                                    return new TextBody();
                                }).ToList()
                            };
                        });

        SetupRepository();
        SetupQuestionnaireCacher();

        foreach (var section in _sections)
        {
            section.Category = _category;
            _category.Sections.Add(new()
            {
                Id = section.Id,
                Category = _category,
                CategoryId = _category.Id
            });
        }

        _category.ContentPages = new(){
            _pageWithCategories
        };

        _pageWithCategories.Content.Add(_category);

        _cmsDbSubstitute.Sections.Returns(_sections.AsQueryable());
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
        _cmsDbSubstitute.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(callinfo =>
                        {
                            var slug = callinfo.ArgAt<string>(0);

                            return _cmsDbSubstitute.Pages.FirstOrDefault(page => string.Equals(page.Slug, slug));
                        });

        var result = await query.GetPageBySlug(LANDING_PAGE_SLUG);

        Assert.NotNull(result);
        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(0).GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(0).ToListAsync(Arg.Any<IQueryable<SectionDbEntity>>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(0).ToListAsync(Arg.Any<IQueryable<ButtonWithEntryReferenceDbEntity>>(), Arg.Any<CancellationToken>());
        _cmsDbSubstitute.ReceivedWithAnyArgs(0).LoadRichTextContentsByParentIds(Arg.Any<IEnumerable<long>>());
    }

    [Fact]
    public async Task Should_Retrieve_Buttons_For_Page_When_Existing()
    {
        GetPageQuery query = CreateGetPageQuery();

        _cmsDbSubstitute.Pages.Returns(_pagesFromDb.AsQueryable());
        _cmsDbSubstitute.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(callinfo =>
                        {
                            var slug = callinfo.ArgAt<string>(0);

                            return _cmsDbSubstitute.Pages.FirstOrDefault(page => string.Equals(page.Slug, slug));
                        });

        var result = await query.GetPageBySlug(_pageWithButton.Slug);

        Assert.NotNull(result);
        Assert.Equal(_pageWithButton.Slug, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(0).GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).ToListAsync(Arg.Any<IQueryable<ButtonWithEntryReferenceDbEntity>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Retrieve_Sections_For_Page_When_Page_Has_Categories()
    {
        GetPageQuery query = CreateGetPageQuery();

        _cmsDbSubstitute.Pages.Returns(_pagesFromDb.AsQueryable());
        _cmsDbSubstitute.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(callinfo =>
                        {
                            var slug = callinfo.ArgAt<string>(0);

                            return _cmsDbSubstitute.Pages.FirstOrDefault(page => string.Equals(page.Slug, slug));
                        });

        _cmsDbSubstitute.Sections.Returns(_sections.AsQueryable());

        var result = await query.GetPageBySlug(_pageWithCategories.Slug);

        Assert.NotNull(result);
        Assert.Equal(_pageWithCategories.Slug, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(0).GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).ToListAsync(Arg.Any<IQueryable<SectionDbEntity>>(), Arg.Any<CancellationToken>());

        var category = result.Content.Where(content => content is Category).Select(cat => cat as Category).FirstOrDefault();

        Assert.NotNull(category);
        Assert.Equal(_sections.Count, category.Sections.Count);

        foreach (var section in _sections)
        {
            var matching = category.Sections.Find(pageSection => section.Id == pageSection.Sys.Id);

            Assert.NotNull(matching);

            foreach (var question in section.Questions)
            {
                var matchingQuestion = matching.Questions.Find(pageQuestion => pageQuestion.Sys.Id == question.Id);
                Assert.NotNull(matchingQuestion);
            }

            foreach (var recommendation in section.Recommendations)
            {
                var matchingRecommendation = matching.Recommendations.Find(pageRecommendation => pageRecommendation.Sys.Id == recommendation.Id);
                Assert.NotNull(matchingRecommendation);
            }
        }
    }

    [Fact]
    public async Task Should_Retrieve_RichText_For_Page()
    {
        GetPageQuery query = CreateGetPageQuery();

        _cmsDbSubstitute.Pages.Returns(_pagesFromDb.AsQueryable());
        _cmsDbSubstitute.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(callinfo =>
                        {
                            var slug = callinfo.ArgAt<string>(0);

                            return _cmsDbSubstitute.Pages.FirstOrDefault(page => string.Equals(page.Slug, slug));
                        });

        var result = await query.GetPageBySlug(_pageWithRichTextContent.Slug);

        Assert.NotNull(result);
        Assert.Equal(_pageWithRichTextContent.Slug, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(0).GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _cmsDbSubstitute.ReceivedWithAnyArgs(1).LoadRichTextContentsByParentIds(Arg.Any<IEnumerable<long>>());
    }

    [Fact]
    public async Task Should_Retrieve_ButtonWithEntryReferences_For_Page_When_Existing()
    {
        GetPageQuery query = CreateGetPageQuery();

        _cmsDbSubstitute.Pages.Returns(_pagesFromDb.AsQueryable());
        _cmsDbSubstitute.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(callinfo =>
                        {
                            var slug = callinfo.ArgAt<string>(0);

                            return _cmsDbSubstitute.Pages.FirstOrDefault(page => string.Equals(page.Slug, slug));
                        });

        var result = await query.GetPageBySlug(_pageWithButton.Slug);

        Assert.NotNull(result);
        Assert.Equal(_pageWithButton.Slug, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(0).GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).ToListAsync(Arg.Any<IQueryable<ButtonWithEntryReferenceDbEntity>>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Should_Retrieve_Page_By_Slug_From_Contentful_When_Db_Page_Not_Found()
    {
        Environment.SetEnvironmentVariable("CONTENTFUL_GET_ENTITY_INT", "4");

        GetPageQuery query = CreateGetPageQuery();

        var emptyList = new List<PageDbEntity>(0);
        _cmsDbSubstitute.Pages.Returns(emptyList.AsQueryable());

        var result = await query.GetPageBySlug(LANDING_PAGE_SLUG);

        Assert.NotNull(result);
        Assert.Equal(LANDING_PAGE_SLUG, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(1).GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Should_Retrieve_Page_By_Slug_From_Contentful_When_Db_Page_Has_No_Content()
    {

        Environment.SetEnvironmentVariable("CONTENTFUL_GET_ENTITY_INT", "4");

        GetPageQuery query = CreateGetPageQuery();

        _cmsDbSubstitute.GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>())
                        .Returns(callinfo =>
                        {
                            var slug = callinfo.ArgAt<string>(0);

                            return _cmsDbSubstitute.Pages.FirstOrDefault(page => string.Equals(page.Slug, slug));
                        });

        var result = await query.GetPageBySlug(TEST_PAGE_SLUG);

        Assert.NotNull(result);
        Assert.Equal(TEST_PAGE_SLUG, result.Slug);

        await _repoSubstitute.ReceivedWithAnyArgs(1).GetEntities<Page>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>());
        await _cmsDbSubstitute.ReceivedWithAnyArgs(1).GetPageBySlug(Arg.Any<string>(), Arg.Any<CancellationToken>());
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

        await Assert.ThrowsAsync<FormatException>(async () => await query.GetPageBySlug(SECTION_SLUG));
    }

}
