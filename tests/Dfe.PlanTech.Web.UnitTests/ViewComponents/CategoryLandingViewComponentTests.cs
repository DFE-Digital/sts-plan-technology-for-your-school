using Dfe.PlanTech.Domain.CategoryLanding;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
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
    public class CategoryLandingViewComponentTests
    {
        private readonly ILogger<CategoryLandingViewComponent> _logger;
        private readonly IGetSubTopicRecommendationQuery _getSubTopicRecommendationQuery;
        private readonly IGetLatestResponsesQuery _getLatestResponsesQuery;
        private readonly IUser _user;
        private readonly IGetSubmissionStatusesQuery _getSubmissionStatusesQuery;
        private readonly CategoryLandingViewComponent _component;
        private Section _completedSection;
        private Section _notStartedSection;
        private Section _inProgressSection;
        private SectionStatusDto _completedSectionStatus;
        private SectionStatusDto _notStartedSectionStatus;
        private SectionStatusDto _inProgressSectionStatus;

        public CategoryLandingViewComponentTests()
        {
            _getSubmissionStatusesQuery = Substitute.For<IGetSubmissionStatusesQuery>();
            _logger = Substitute.For<ILogger<CategoryLandingViewComponent>>();
            _getSubTopicRecommendationQuery = Substitute.For<IGetSubTopicRecommendationQuery>();
            _getLatestResponsesQuery = Substitute.For<IGetLatestResponsesQuery>();
            _user = Substitute.For<IUser>();

            var viewContext = new ViewContext();
            var viewComponentContext = new ViewComponentContext
            {
                ViewContext = viewContext
            };

            _component = new CategoryLandingViewComponent(
                _logger,
                _getSubmissionStatusesQuery,
                _getSubTopicRecommendationQuery,
                _getLatestResponsesQuery,
                _user)
            {
                ViewComponentContext = viewComponentContext
            };

            _completedSection = new Section()
            {
                Sys = new SystemDetails() { Id = "completedSection" },
                Name = "Completed Section",
                ShortDescription = "Description of completed section",
                InterstitialPage = new Page() { Slug = "section-completed" }
            };

            _notStartedSection = new Section()
            {
                Sys = new SystemDetails() { Id = "notStartedSection" },
                Name = "Not Started Section",
                ShortDescription = "Description of not started section",
                InterstitialPage = new Page() { Slug = "section-not-started" }
            };

            _inProgressSection = new Section()
            {
                Sys = new SystemDetails() { Id = "inProgressSection" },
                Name = "In Progress Section",
                ShortDescription = "Description of in progress section",
                InterstitialPage = new Page() { Slug = "section-in-progress" }
            };

            _completedSectionStatus = new SectionStatusDto
            {
                SectionId = _completedSection.Sys.Id,
                Completed = true,
                DateCreated = DateTime.Now,
                LastCompletionDate = DateTime.Now,
                DateUpdated = DateTime.Now
            };

            _notStartedSectionStatus = new SectionStatusDto
            {
                SectionId = _notStartedSection.Sys.Id,
                Completed = false,
            };

            _inProgressSectionStatus = new SectionStatusDto
            {
                SectionId = _inProgressSection.Sys.Id,
                Completed = false,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
            };
        }

        [Fact]
        public async Task ShouldReturnViewWithViewModel_WhenCategoryIsValid()
        {

            var category = new Category()
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "Category-Test-Id" },
                Sections = new List<Section> { _completedSection, _notStartedSection, _inProgressSection }
            };

            category.SectionStatuses = new List<SectionStatusDto> { _completedSectionStatus, _notStartedSectionStatus, _inProgressSectionStatus };

            int establishmentId = 99;
            _user.GetEstablishmentId().Returns(establishmentId);
            _getLatestResponsesQuery.GetLatestResponses(establishmentId, _completedSection.Sys.Id, true)
                .Returns(new SubmissionResponsesDto { Responses = new List<QuestionWithAnswer>() });
            _getSubTopicRecommendationQuery.GetSubTopicRecommendation(_completedSection.Sys.Id)
                .Returns(new SubtopicRecommendation
                {
                    Section = new RecommendationSection
                    {
                        Chunks = new List<RecommendationChunk> { new RecommendationChunk { } }
                    },
                    Subtopic = _completedSection
                });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections)
                .Returns(new List<SectionStatusDto> { _completedSectionStatus, _notStartedSectionStatus, _inProgressSectionStatus });

            var result = await _component.InvokeAsync(category, "test-category") as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);
            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result.ViewData.Model);
            Assert.Equal("Test Category", viewModel.CategoryName);
            Assert.Equal("test-category", viewModel.CategorySlug);
            Assert.False(viewModel.AllSectionsCompleted);
            Assert.True(viewModel.AnySectionsCompleted);
            Assert.Equal(category.Sections.Count, viewModel.CategoryLandingSections.Count);
            Assert.Null(viewModel.NoSectionsErrorRedirectUrl);
            Assert.Null(viewModel.ProgressRetrievalErrorMessage);
        }

        [Fact]
        public async Task ShouldSetNoSectionsErrorRedirect_WhenCategoryHasNoSections()
        {
            var category = new Category
            {
                Sys = new SystemDetails { Id = "category1" },
                Sections = []
            };
            var slug = "category1-slug";

            var result = await _component.InvokeAsync(category, slug) as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);
            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result.ViewData.Model);
            Assert.Equal("ServiceUnavailable", viewModel.NoSectionsErrorRedirectUrl);
        }

        [Fact]
        public async Task ShouldSetProgressRetrievalErrorMessage_WhenRetrievalErrorIsTrue()
        {
            var category = new Category
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "category1" },
                Sections = new List<Section> { _completedSection, _completedSection },
                RetrievalError = true,
            };

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections, Arg.Any<int?>())
                .Throws(new Exception("Progress retrieval failure simulation"));

            _user.GetEstablishmentId().Returns(99);
            _getLatestResponsesQuery.GetLatestResponses(99, _completedSection.Sys.Id, true)
                .Returns(new SubmissionResponsesDto { Responses = new List<QuestionWithAnswer>() });

            _getSubTopicRecommendationQuery.GetSubTopicRecommendation(_completedSection.Sys.Id)
            .Returns(new SubtopicRecommendation
            {
                Section = new RecommendationSection { }
            });

            var result = await _component.InvokeAsync(category, "test-slug") as ViewViewComponentResult;

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);
            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result.ViewData.Model);
            Assert.Equal("Unable to retrieve progress, please refresh your browser.", viewModel.ProgressRetrievalErrorMessage);
        }

        [Fact]
        public async Task ShouldSetAllSectionsCompletedToTrue_WhenAllSectionsAreCompleted()
        {
            var category = new Category
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "Category-Test-Id" },
                Sections = new List<Section> { _completedSection, _completedSection },
            };

            var sectionStatuses = new List<SectionStatusDto> { _completedSectionStatus, _completedSectionStatus };
            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections)
                .Returns(sectionStatuses);

            var result = await _component.InvokeAsync(category, "test-slug") as ViewViewComponentResult;
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);
            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result.ViewData.Model);
            Assert.True(viewModel.AllSectionsCompleted);
            Assert.True(viewModel.AnySectionsCompleted);
            Assert.Equal(2, viewModel.CategoryLandingSections.Count);
        }

        [Fact]
        public async Task ShouldSetAnySectionsCompletedToTrue_WhenAtLeastOneSectionCompleted()
        {
            var category = new Category
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "Category-Test-Id" },
                Sections = new List<Section> { _completedSection, _notStartedSection, _notStartedSection },
            };

            var sectionStatuses = new List<SectionStatusDto> { _completedSectionStatus, _notStartedSectionStatus, _notStartedSectionStatus };
            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections)
                .Returns(sectionStatuses);

            var result = await _component.InvokeAsync(category, "test-slug") as ViewViewComponentResult;
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);
            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result.ViewData.Model);
            Assert.False(viewModel.AllSectionsCompleted);
            Assert.True(viewModel.AnySectionsCompleted);
            Assert.Equal(3, viewModel.CategoryLandingSections.Count);
        }

        [Fact]
        public async Task ShouldSetAnySectionsCompletedAndAllSectionsCompletedToFalse_WhenNoSectionsCompleted()
        {
            var category = new Category
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "Category-Test-Id" },
                Sections = new List<Section> { _inProgressSection, _notStartedSection },
            };

            var sectionStatuses = new List<SectionStatusDto> { _inProgressSectionStatus, _notStartedSectionStatus };
            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections)
                .Returns(sectionStatuses);

            var result = await _component.InvokeAsync(category, "test-slug") as ViewViewComponentResult;
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);
            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result.ViewData.Model);
            Assert.False(viewModel.AllSectionsCompleted);
            Assert.False(viewModel.AnySectionsCompleted);
            Assert.Equal(2, viewModel.CategoryLandingSections.Count);
        }

        [Fact]
        public async Task ShouldLogError_WhenSectionHasNoSlug()
        {
            var category = new Category
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "category-1" },
                Sections = new List<Section>
                {
                    new Section
                    {
                        Name = "Section Without Slug",
                        Sys = new SystemDetails { Id = "section1" },
                        ShortDescription = "This section has no slug",
                        InterstitialPage = new Page { Slug = null! }
                    }
                }
            };

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections, Arg.Any<int?>())
                .Returns(new List<SectionStatusDto>
                {
                    new SectionStatusDto { SectionId = "section1", Completed = false }
                });

            _user.GetEstablishmentId().Returns(99);
            _getLatestResponsesQuery.GetLatestResponses(99, "section1", true)
                .Returns(new SubmissionResponsesDto { Responses = new List<QuestionWithAnswer>() });

            _getSubTopicRecommendationQuery.GetSubTopicRecommendation("section1")
                .Returns(new SubtopicRecommendation
                {
                    Section = new RecommendationSection { }
                });

            var result = await _component.InvokeAsync(category, "test-slug") as ViewViewComponentResult;

            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result?.ViewData?.Model);
            Assert.Single(viewModel.CategoryLandingSections);
            var section = viewModel.CategoryLandingSections.First();
            Assert.Equal("Section Without Slug", section.Name);
            Assert.Null(section.Slug);
        }

        [Fact]
        public async Task ShouldSetNoRecommendationFoundErrorMessage_WhenGetLatestResponsesQueryFails()
        {
            string sectionName = "Test section";
            var category = new Category
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "category-1" },
                Sections = new List<Section>
                {
                    new Section
                    {
                        Name = sectionName,
                        Sys = new SystemDetails { Id = "section1" },
                        ShortDescription = "This section has no recommendations",
                        InterstitialPage = new Page { Slug = "test-section" }
                    }
                }
            };

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections, Arg.Any<int?>())
                .Returns(new List<SectionStatusDto>
                {
                    new SectionStatusDto { SectionId = "section1", Completed = false }
                });

            _user.GetEstablishmentId().Returns(99);
            _getLatestResponsesQuery.GetLatestResponses(99, "section1", true)
                .Returns((SubmissionResponsesDto)null!);

            var result = await _component.InvokeAsync(category, "test-slug") as ViewViewComponentResult;

            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result?.ViewData?.Model);
            var section = viewModel.CategoryLandingSections.First();
            Assert.Null(section.Recommendations.Answers);
            Assert.Null(section.Recommendations.Chunks);
            Assert.Equal($"Unable to retrieve {sectionName} recommendation", section.Recommendations.NoRecommendationFoundErrorMessage);
        }

        [Fact]
        public async Task ShouldSetNoRecommendationFoundErrorMessage_WhenGetSubtopicRecommendationQueryFails()
        {
            string sectionName = "Test section";
            var category = new Category
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "category-1" },
                Sections = new List<Section>
                {
                    new Section
                    {
                        Name = sectionName,
                        Sys = new SystemDetails { Id = "section1" },
                        ShortDescription = "This section has no recommendations",
                        InterstitialPage = new Page { Slug = "test-section" }
                    }
                }
            };

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections, Arg.Any<int?>())
                .Returns(new List<SectionStatusDto>
                {
                    new SectionStatusDto { SectionId = "section1", Completed = false }
                });

            _user.GetEstablishmentId().Returns(99);

            _getLatestResponsesQuery.GetLatestResponses(99, "section1", true)
                .Returns(new SubmissionResponsesDto { Responses = new List<QuestionWithAnswer>() });

            _getSubTopicRecommendationQuery.GetSubTopicRecommendation("section1")
                .Returns((SubtopicRecommendation)null!);

            var result = await _component.InvokeAsync(category, "test-slug") as ViewViewComponentResult;

            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result?.ViewData?.Model);
            var section = viewModel.CategoryLandingSections.First();
            Assert.Null(section.Recommendations.Answers);
            Assert.Null(section.Recommendations.Chunks);
            Assert.Equal($"Unable to retrieve {sectionName} recommendation", section.Recommendations.NoRecommendationFoundErrorMessage);
        }

        [Fact]

        public async Task ShouldReturnRecommendations_WhenSectionIsCompleted()
        {
            var question2 = new Question { Sys = new SystemDetails { Id = "question2" } };
            var question1 = new Question
            {
                Sys = new SystemDetails
                {
                    Id = "question1"
                },
                Answers = new List<Answer>
                {
                    new Answer
                    {
                        Sys = new SystemDetails
                        {
                            Id = "answer1"
                        },
                        NextQuestion = question2
                    }
                }
            };
            var answer1 = new Answer { Sys = new SystemDetails { Id = "answer1" }, NextQuestion = question2 };
            var answer2 = new Answer { Sys = new SystemDetails { Id = "answer2" } };

            var responses = new List<QuestionWithAnswer>
            {
               new QuestionWithAnswer
                {
                    QuestionRef = "question1",
                    QuestionText = "Are you a school?",
                    AnswerRef = "answer1",
                    AnswerText = "Yes",
                    DateCreated = DateTime.UtcNow,
                    QuestionSlug = "are-you-a-school"
                },
                new QuestionWithAnswer
                {
                    QuestionRef = "question2",
                    QuestionText = "Have you got broadband?",
                    AnswerRef = "answer2",
                    AnswerText = "No",
                    DateCreated = DateTime.UtcNow,
                    QuestionSlug = "have-you-got-broadband"
                }
            };

            var recommendationChunks = new List<RecommendationChunk>
            {
                new RecommendationChunk
                {
                    Header = "Chunk 1",
                    Answers = new List<Answer> { answer1 }
                },
                new RecommendationChunk
                {
                    Header = "Chunk 2",
                    Answers = new List<Answer> { answer2 }
                }
            };

            var recommendationSection = new RecommendationSection
            {
                Answers = new List<Answer> { answer1, answer2 },
                Chunks = recommendationChunks
            };

            var section = new Section
            {
                Name = "Completed Section",
                Sys = new SystemDetails { Id = "section-1" },
                InterstitialPage = new Page { Slug = "slug-1" },
                ShortDescription = "Short description",
                Questions = new List<Question> { question1, question2 }
            };

            var category = new Category
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "Category-Test-Id" },
                Sections = new List<Section> { section },
                SectionStatuses = new List<SectionStatusDto>
                {
                    new SectionStatusDto
                    {
                        SectionId = "section-1",
                        Completed = true,
                        DateCreated = DateTime.UtcNow,
                        DateUpdated = DateTime.UtcNow,
                        LastCompletionDate = DateTime.UtcNow
                    }
                }
            };

            int establishmentId = 99;
            _user.GetEstablishmentId().Returns(establishmentId);

            _getLatestResponsesQuery.GetLatestResponses(establishmentId, section.Sys.Id, true)
                .Returns(new SubmissionResponsesDto { Responses = responses });

            _getSubTopicRecommendationQuery.GetSubTopicRecommendation(section.Sys.Id)
                .Returns(new SubtopicRecommendation
                {
                    Section = recommendationSection,
                    Subtopic = section
                });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections)
                .Returns(Task.FromResult(category.SectionStatuses.ToList()));

            var result = await _component.InvokeAsync(category, "test-category") as ViewViewComponentResult;


            var orderedResponses = section.GetOrderedResponsesForJourney(responses).ToList();
            Assert.NotEmpty(orderedResponses);
            Assert.All(orderedResponses, r => Assert.False(string.IsNullOrWhiteSpace(r.AnswerRef)));

            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);

            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result.ViewData.Model);
            var landingSection = viewModel.CategoryLandingSections.First();

            Assert.NotNull(landingSection.Recommendations);
            Assert.Equal(2, landingSection.Recommendations.Chunks.Count);
            Assert.Equal("Chunk 1", landingSection.Recommendations.Chunks[0].Header);
            Assert.Equal("Chunk 2", landingSection.Recommendations.Chunks[1].Header);
        }

        [Fact]
        public async Task ShouldYieldCorrectCategoryLandingSections()
        {

            var category = new Category
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "Category-Test-Id" },
                Sections = new List<Section> { _completedSection, _notStartedSection }
            };

            category.SectionStatuses = new List<SectionStatusDto> { _completedSectionStatus, _notStartedSectionStatus };

            int establishmentId = 99;
            _user.GetEstablishmentId().Returns(establishmentId);

            _getLatestResponsesQuery.GetLatestResponses(establishmentId, Arg.Any<string>(), true)
                .Returns(new SubmissionResponsesDto { Responses = new List<QuestionWithAnswer>() });

            _getSubTopicRecommendationQuery.GetSubTopicRecommendation(Arg.Any<string>())
                .Returns(new SubtopicRecommendation
                {
                    Section = new RecommendationSection
                    {
                        Chunks = new List<RecommendationChunk>()
                    },
                    Subtopic = _completedSection
                });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections)
                .Returns((List<SectionStatusDto>)category.SectionStatuses);

            var result = await _component.InvokeAsync(category, "test-category") as ViewViewComponentResult;

            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result?.ViewData?.Model);
            Assert.Equal(category.Sections.Count, viewModel.CategoryLandingSections.Count);
            foreach (var section in category.Sections)
            {
                Assert.Contains(viewModel.CategoryLandingSections, s => s.Name == section.Name);
            }
        }


        [Theory]
        [InlineData(null, true, SectionProgressStatus.RetrievalError, null)] // retrieval error
        [InlineData(null, false, SectionProgressStatus.RetrievalError, null)] // missing slug
        [InlineData("slug-1", false, SectionProgressStatus.Completed, "2025-07-16", "2025-07-16")] // completed
        [InlineData("slug-1", false, SectionProgressStatus.InProgress, null, "2025-07-15")] // in progress
        [InlineData("slug-1", false, SectionProgressStatus.NotStarted, null)] // not started
        public async Task ShouldSetCorrectProgressStatusForSections(
            string? slug,
            bool retrievalError,
            SectionProgressStatus expectedStatus,
            string? lastCompletionDateStr,
            string? dateUpdatedStr = null)
        {
            var section = new Section
            {
                Name = "Test Section",
                Sys = new SystemDetails { Id = "section-1" },
                InterstitialPage = slug != null ? new Page { Slug = slug } : null,
                ShortDescription = "Short description"
            };

            var category = new Category
            {
                Header = new Header { Text = "Test Category" },
                Sys = new SystemDetails { Id = "Category-Test-Id" },
                Sections = new List<Section> { section },
                Completed = expectedStatus == SectionProgressStatus.Completed ? 1 : 0,
                RetrievalError = retrievalError
            };

            var sectionStatus = (expectedStatus == SectionProgressStatus.NotStarted || expectedStatus == SectionProgressStatus.RetrievalError)
                ? null
                : new SectionStatusDto
                {
                    SectionId = "section-1",
                    LastCompletionDate = lastCompletionDateStr != null ? DateTime.Parse(lastCompletionDateStr) : null,
                    DateUpdated = dateUpdatedStr != null ? DateTime.Parse(dateUpdatedStr) : DateTime.Now
                };

            if (sectionStatus != null)
            {
                category.SectionStatuses = new List<SectionStatusDto> { sectionStatus };
            }

            int establishmentId = 99;
            _user.GetEstablishmentId().Returns(establishmentId);

            _getLatestResponsesQuery.GetLatestResponses(establishmentId, section.Sys.Id, true)
                .Returns(new SubmissionResponsesDto { Responses = new List<QuestionWithAnswer>() });

            _getSubTopicRecommendationQuery.GetSubTopicRecommendation(section.Sys.Id)
                .Returns(new SubtopicRecommendation
                {
                    Section = new RecommendationSection
                    {
                        Chunks = new List<RecommendationChunk>()
                    },
                    Subtopic = section
                });

            _getSubmissionStatusesQuery.GetSectionSubmissionStatuses(category.Sections)
                .Returns(Task.FromResult(category.SectionStatuses.ToList() ?? new List<SectionStatusDto>()));

            var result = await _component.InvokeAsync(category, "test-category") as ViewViewComponentResult;

            var viewModel = Assert.IsType<CategoryLandingViewComponentViewModel>(result?.ViewData?.Model);
            var landingSection = viewModel.CategoryLandingSections.First();

            Assert.Equal(expectedStatus, landingSection.ProgressStatus);
        }
    }
}
