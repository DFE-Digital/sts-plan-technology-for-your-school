using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Queries;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents
{
    public class CategorySectionViewComponentTests
    {
        private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;
        private readonly CategorySectionViewComponent _categorySectionViewComponent;
        private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery;
        private readonly ISystemTime _systemTime;

        private Category _category;
        private readonly Category _categoryTwo;
        private readonly ILogger<CategorySectionViewComponent> _loggerCategory;

        private readonly SubtopicRecommendation? _subtopic = new SubtopicRecommendation()
        {
            Intros = new List<RecommendationIntro>()
            {
                new RecommendationIntro()
                {
                    Header = new Header()
                    {
                        Text = "I'm a high maturity recommendation for subtopic 1",
                    },
                    Slug = "intro-slug",
                    Maturity = "High",
                }
            },
            Section = new RecommendationSection()
            {
                Chunks = new List<RecommendationChunk>()
                {
                    new RecommendationChunk()
                    {
                        Answers = new List<Answer>()
                        {
                            new Answer()
                            {
                                Sys = new SystemDetails()
                                {
                                    Id = "ref1"
                                }
                            }
                        }
                    }
                }
            },
            Subtopic = new Section()
            {
                InterstitialPage = new Page()
                {
                    Slug = "subtopic-slug"
                },
                Sys = new SystemDetails()
                {
                    Id = "Section1"
                }
            }
        };

        private readonly SubtopicRecommendation? _subtopicTwo = new SubtopicRecommendation()
        {
            Intros = new List<RecommendationIntro>()
            {
                new RecommendationIntro()
                {
                    Header = new Header()
                    {
                        Text = "I'm a high maturity recommendation for subtopic 2",
                    },
                    Slug = "intro-slug",
                    Maturity = "High",
                }
            },
            Section = new RecommendationSection()
            {
                Chunks = new List<RecommendationChunk>()
                {
                    new RecommendationChunk()
                    {
                        Answers = new List<Answer>()
                        {
                            new Answer()
                            {
                                Sys = new SystemDetails()
                                {
                                    Id = "ref1"
                                }
                            }
                        }
                    }
                }
            },
            Subtopic = new Section()
            {
                InterstitialPage = new Page()
                {
                    Slug = "subtopic-slug"
                },
                Sys = new SystemDetails()
                {
                    Id = "Section2"
                }
            }
        };

        private readonly List<SubtopicRecommendation> _subtopicRecommendations = [];

        public CategorySectionViewComponentTests()
        {
            _getSubmissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();
            _loggerCategory = Substitute.For<ILogger<CategorySectionViewComponent>>();
            _getSubTopicRecommendationQuery = Substitute.For<IGetSubTopicRecommendationQuery>();
            _systemTime = Substitute.For<ISystemTime>();

            _subtopicRecommendations.Add(_subtopic);
            _subtopicRecommendations.Add(_subtopicTwo);

            _getSubTopicRecommendationQuery.GetRecommendationsViewDto(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((callinfo) =>
                {
                    var subtopic = callinfo.ArgAt<string>(0);
                    var maturity = callinfo.ArgAt<string>(1);

                    var matching = _subtopicRecommendations.FirstOrDefault(rec => rec.Subtopic.Sys.Id == subtopic);

                    if (matching == null)
                    {
                        return null;
                    }

                    var introForMaturity = matching.Intros.FirstOrDefault(intro => intro.Maturity == maturity);

                    if (introForMaturity == null)
                    {
                        return null;
                    }

                    return new RecommendationsViewDto(introForMaturity.Slug, introForMaturity.HeaderText);
                });

            var viewContext = new ViewContext();

            var viewComponentContext = new ViewComponentContext
            {
                ViewContext = viewContext
            };

            _categorySectionViewComponent = new CategorySectionViewComponent(
                _loggerCategory,
                _getSubmissionStatusesQuery,
                _getSubTopicRecommendationQuery,
                _systemTime)
            {
                ViewComponentContext = viewComponentContext
            };

            _category = new Category()
            {
                Completed = 1,
                Sys = new()
                {
                    Id = "Category-Test-Id"
                },
                Sections = new(){
                {
                    new ()
                    {
                        Sys = new SystemDetails() { Id = "Section1" },
                        Name = "Test Section 1",
                        InterstitialPage = new Page()
                        {
                            Slug = "section-1",
                        },
                    }
                }
            }
            };
            _categoryTwo = new Category()
            {
                Completed = 1,
                Sections = [
                    new()
                    {
                        Sys = new SystemDetails() { Id = "Section2" },
                        Name = "Test Section 2",
                        InterstitialPage = new Page
                        {
                            Slug = "test-slug"
                        }
                    }
                ]
            };
        }

        [Theory]
        [InlineData("2015/10/15", "last completed 15 Oct 2015")]
        [InlineData("2015/10/16 12:13:00", "last completed 1:13pm")] // British summer time GMT + 1
        public async Task Returns_CategorySectionInfo_If_Slug_Exists_And_SectionIsCompleted(string utcTime, string expectedBadge)
        {
            _systemTime.Today.Returns(_ => new DateTime(2015, 10, 16, 0, 0, 0, DateTimeKind.Utc));
            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = true,
                DateUpdated = DateTime.Parse(utcTime),
            });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sys.Id).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);

            Assert.Equal(1, unboxed.CompletedSectionCount);
            Assert.Equal(1, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Equal("section-1", categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Equal("grey", categorySectionDto.Tag.Colour);
            Assert.Equal(expectedBadge, categorySectionDto.Tag.Text);
            Assert.Null(categorySectionDto.ErrorMessage);
        }

        [Theory]
        [InlineData("2015/03/05", "in progress 5 Mar 2015")]
        [InlineData("2015/03/06 10:13:00", "in progress 10:13am")] // GMT
        public async Task Returns_CategorySelectionInfo_If_Slug_Exists_And_SectionIsNotCompleted(string utcTime, string expectedBadge)
        {
            _systemTime.Today.Returns(_ => new DateTime(2015, 3, 6, 0, 0, 0, DateTimeKind.Utc));
            _category.Completed = 0;

            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = false,
                DateUpdated = DateTime.Parse(utcTime)
            });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sys.Id).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);

            Assert.Equal(0, unboxed.CompletedSectionCount);
            Assert.Equal(1, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Equal("section-1", categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Equal("grey", categorySectionDto.Tag.Colour);
            Assert.Equal(expectedBadge, categorySectionDto.Tag.Text);
            Assert.Null(categorySectionDto.ErrorMessage);
        }

        [Fact]
        public async Task Returns_CategorySelectionInfo_If_Section_IsNotStarted()
        {
            _category.Completed = 0;

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sys.Id).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);

            Assert.Equal(0, unboxed.CompletedSectionCount);
            Assert.Equal(1, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Equal("section-1", categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Equal("grey", categorySectionDto.Tag.Colour);
            Assert.Equal("not started", categorySectionDto.Tag.Text);
            Assert.Null(categorySectionDto.ErrorMessage);
        }

        [Fact]
        public async Task Returns_NullSlug_And_ErrorMessage_In_CategorySectionInfo_If_SlugDoesNotExist()
        {
            _category.Sections[0] = new Section()
            {
                Sys = new SystemDetails() { Id = "Section1" },
                Name = "Test Section 1",
                InterstitialPage = new Page()
                {
                    Slug = null!,
                }
            };

            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = true
            });

            _ = _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sys.Id).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);

            Assert.Equal(1, unboxed.CompletedSectionCount);
            Assert.Equal(1, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Null(categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Null(categorySectionDto.Tag.Colour);
            Assert.Null(categorySectionDto.Tag.Text);
            Assert.NotNull(categorySectionDto.ErrorMessage);
            Assert.Equal("Test Section 1 unavailable", categorySectionDto.ErrorMessage);
        }

        [Fact]
        public async Task Returns_ProgressRetrievalError_When_ProgressCanNotBeRetrieved()
        {
            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(Arg.Any<string>())
                                        .ThrowsAsync(new Exception("Error occurred fection sections"));

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);
            Assert.Equal("Unable to retrieve progress, please refresh your browser.", unboxed.ProgressRetrievalErrorMessage);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.NotNull(categorySectionDtoList);

            categorySectionDtoList = categorySectionDtoList.ToList();
            Assert.NotEmpty(categorySectionDtoList);

            var categorySectionDto = categorySectionDtoList.FirstOrDefault();

            Assert.NotNull(categorySectionDto);

            Assert.Equal("section-1", categorySectionDto.Slug);
            Assert.Equal("Test Section 1", categorySectionDto.Name);
            Assert.Equal("red", categorySectionDto.Tag.Colour);
            Assert.Equal("unable to retrieve status", categorySectionDto.Tag.Text);
            Assert.Null(categorySectionDto.ErrorMessage);
        }

        [Fact]
        public async Task Returns_NoSectionsErrorRedirectUrl_If_SectionsAreEmpty()
        {
            _category = new Category()
            {
                Completed = 0,
                Sections = new(),
                Sys = new()
                {
                    Id = "empty-sections-category"
                }
            };

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sys.Id).Returns([.. _category.SectionStatuses]);

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model;
            Assert.NotNull(model);

            var unboxed = model as CategorySectionViewComponentViewModel;
            Assert.NotNull(unboxed);
            Assert.Equal("ServiceUnavailable", unboxed.NoSectionsErrorRedirectUrl);
            Assert.Equal(0, unboxed.TotalSectionCount);

            var categorySectionDtoList = unboxed.CategorySectionDto;

            Assert.Null(categorySectionDtoList);
        }


        [Fact]
        public async Task Returns_RecommendationInfo_If_It_Exists_ForMaturity()
        {
            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = true,
                LastMaturity = "High"
            });

            _getSubTopicRecommendationQuery.GetSubTopicRecommendation(Arg.Any<string>()).Returns(_subtopic);
            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sys.Id).Returns(_category.SectionStatuses.ToList());

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model as CategorySectionViewComponentViewModel;
            Assert.NotNull(model);

            Assert.NotNull(_subtopic);
            Assert.NotEmpty(model.CategorySectionDto);
            var recommendation = model.CategorySectionDto.First().Recommendation;
            Assert.NotNull(recommendation);
            Assert.Equal(_subtopic.Intros[0].Slug, recommendation.RecommendationSlug);
            Assert.Equal(_subtopic.Intros[0].HeaderText, recommendation.RecommendationDisplayName);
            Assert.Null(recommendation.NoRecommendationFoundErrorMessage);
        }

        [Fact]
        public async Task Returns_RecommendationInfo_And_Logs_Error_If_Exception_Thrown_By_Get_Category()
        {
            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = true,
                LastMaturity = "High"
            });

            _getSubTopicRecommendationQuery.GetSubTopicRecommendation(Arg.Any<string>()).Returns(_subtopic);
            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sys.Id).Throws(new Exception("test"));

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model as CategorySectionViewComponentViewModel;
            Assert.NotNull(model);

            Assert.NotNull(_subtopic);
            Assert.NotEmpty(model.CategorySectionDto);
            var recommendation = model.CategorySectionDto.First().Recommendation;
            Assert.NotNull(recommendation);

            Assert.NotNull(_subtopic);
            Assert.Equal(_subtopic.Intros[0].Slug, recommendation.RecommendationSlug);
            Assert.Equal(_subtopic.Intros[0].HeaderText, recommendation.RecommendationDisplayName);
            Assert.Null(recommendation.NoRecommendationFoundErrorMessage);
            _loggerCategory.ReceivedWithAnyArgs(1).LogError("An exception has occurred while trying to retrieve section progress with the following message - test");
        }

        [Fact]
        public async Task Returns_NullRecommendationInfo_If_No_RecommendationPage_Exists_ForMaturity()
        {
            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = true,
                LastMaturity = "Low",
            });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sys.Id).Returns(_category.SectionStatuses.ToList());

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model as CategorySectionViewComponentViewModel;
            Assert.NotNull(model);

            Assert.NotNull(_subtopic);
            Assert.NotEmpty(model.CategorySectionDto);
            var recommendation = model.CategorySectionDto.First().Recommendation;
            Assert.NotNull(recommendation);

            Assert.Null(recommendation.RecommendationSlug);
            Assert.Null(recommendation.RecommendationDisplayName);
            Assert.NotNull(recommendation.NoRecommendationFoundErrorMessage);
        }

        [Fact]
        public async Task DoesNotReturn_RecommendationInfo_If_Section_Has_Never_Been_Completed()
        {
            _category.Completed = 0;
            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = false,
                LastMaturity = null
            });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sys.Id).Returns(_category.SectionStatuses.ToList());

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model as CategorySectionViewComponentViewModel;
            Assert.NotNull(model);

            Assert.NotNull(_subtopic);
            Assert.NotEmpty(model.CategorySectionDto);
            var recommendation = model.CategorySectionDto.First().Recommendation;
            Assert.NotNull(recommendation);
            Assert.Null(recommendation.RecommendationSlug);
            Assert.Null(recommendation.RecommendationDisplayName);
        }

        [Fact]
        public async Task Returns_RecommendationInfo_If_Incomplete_Section_Is_Previously_Completed()
        {
            _category.SectionStatuses.Add(new SectionStatusDto()
            {
                SectionId = "Section1",
                Completed = false,
                LastMaturity = Maturity.High.ToString(),
                DateCreated = DateTime.Now
            });

            _getSubTopicRecommendationQuery.GetSubTopicRecommendation(Arg.Any<string>()).Returns(_subtopic);
            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(_category.Sys.Id).Returns(_category.SectionStatuses.ToList());

            var result = await _categorySectionViewComponent.InvokeAsync(_category) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var model = result.ViewData.Model as CategorySectionViewComponentViewModel;
            Assert.NotNull(model);

            Assert.NotNull(_subtopic);
            Assert.NotEmpty(model.CategorySectionDto);
            var recommendation = model.CategorySectionDto.First().Recommendation;
            Assert.NotNull(recommendation);
            Assert.Equal(_subtopic.Intros[0].Slug, recommendation.RecommendationSlug);
            Assert.Equal(_subtopic.Intros[0].HeaderText, recommendation.RecommendationDisplayName);
            Assert.Null(recommendation.NoRecommendationFoundErrorMessage);
        }
    }
}
