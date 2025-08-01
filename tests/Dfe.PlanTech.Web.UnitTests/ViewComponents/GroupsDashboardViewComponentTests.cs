using System.Reflection;
using Dfe.PlanTech.Domain.CategorySection;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Groups.Interfaces;
using Dfe.PlanTech.Domain.Groups.Models;
using Dfe.PlanTech.Domain.Helpers;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Questionnaire.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents
{
    public class GroupsDashboardViewComponentTests
    {
        private readonly ILogger<GroupsDashboardViewComponent> _logger =
            Substitute.For<ILogger<GroupsDashboardViewComponent>>();

        private readonly IGetSubmissionStatusesQuery _submissionStatusesQuery =
            Substitute.For<IGetSubmissionStatusesQuery>();

        private readonly IGetGroupSelectionQuery _groupSelectionQuery = Substitute.For<IGetGroupSelectionQuery>();
        private readonly IUser _user = Substitute.For<IUser>();
        private readonly ISystemTime _systemTime = Substitute.For<ISystemTime>();

        private readonly IGetSubTopicRecommendationQuery _recommendationQuery =
            Substitute.For<IGetSubTopicRecommendationQuery>();

        private GroupsDashboardViewComponent CreateComponent()
        {
            return new GroupsDashboardViewComponent(
                _logger,
                _submissionStatusesQuery,
                _groupSelectionQuery,
                _user,
                _systemTime,
                _recommendationQuery
            );
        }

        [Fact]
        public async Task InvokeAsync_ReturnsRedirect_WhenCategoryHasNoSections()
        {
            var category = new Category
            {
                Sys = new SystemDetails { Id = "cat1" },
                Sections = new List<Section>()
            };

            var component = CreateComponent();

            var result = await component.InvokeAsync(category);

            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            var model = Assert.IsType<GroupsDashboardViewComponentViewModel>(viewResult?.ViewData?.Model);
            Assert.Equal("ServiceUnavailable", model.NoSectionsErrorRedirectUrl);
        }

        [Fact]
        public async Task RetrieveSectionStatuses_ReturnsError_WhenExceptionThrown()
        {
            var category = new Category
            {
                Sections = new List<Section> { new Section { Sys = new SystemDetails { Id = "sec1" } } }
            };

            _submissionStatusesQuery
                .GetSectionSubmissionStatuses(Arg.Any<IEnumerable<Section>>(), Arg.Any<int>())
                .Throws(new Exception("Database error"));

            var result = await SubmissionStatusHelpers.RetrieveSectionStatuses(category, _logger, _submissionStatusesQuery, 1);

            Assert.True(result.RetrievalError);
        }

        [Fact]
        public async Task RetrieveSectionStatuses_PopulatesStatuses_WhenSuccessful()
        {
            var section = new Section { Sys = new SystemDetails { Id = "s1" } };
            var category = new Category { Sections = new List<Section> { section } };

            var statuses = new List<SectionStatusDto> { new SectionStatusDto { SectionId = "s1", Completed = true, LastCompletionDate = new DateTime() } };

            _submissionStatusesQuery
                .GetSectionSubmissionStatuses(Arg.Any<IEnumerable<Section>>(), Arg.Any<int>())
                .Returns(statuses);

            var result = await SubmissionStatusHelpers.RetrieveSectionStatuses(category, _logger, _submissionStatusesQuery, 1);

            Assert.False(result.RetrievalError);
            Assert.Single(result.SectionStatuses);
            Assert.Equal(1, result.Completed);
        }

        [Fact]
        public async Task GenerateViewModel_HandlesEmptyContent_Gracefully()
        {
            var category = new Category
            {
                Sections = new List<Section> { new Section { Sys = new SystemDetails { Id = "s1" } } },
            };

            _user.GetCurrentUserId().Returns(1);
            _user.GetEstablishmentId().Returns(10);
            _groupSelectionQuery.GetLatestSelectedGroupSchool(1, 10)
                .Returns(new GroupReadActivityDto() { SelectedEstablishmentId = 20 });

            _submissionStatusesQuery.GetSectionSubmissionStatuses(Arg.Any<IEnumerable<Section>>(), 20)
                .Returns(new List<SectionStatusDto>());

            var component = CreateComponent();

            var result = await component.InvokeAsync(category);

            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            var model = Assert.IsType<GroupsDashboardViewComponentViewModel>(viewResult?.ViewData?.Model);
            Assert.IsType<MissingComponent>(model.Description);
        }

        [Fact]
        public async Task GetCategorySectionRecommendationDto_ReturnsFallback_WhenNoRecommendation()
        {
            var section = new Section
            {
                Name = "Section A",
                InterstitialPage = new Page { Slug = "slug" },
                Sys = new SystemDetails { Id = "s1" }
            };

            var status = new SectionStatusDto
            {
                SectionId = "s1",
                LastMaturity = "NonMatching"
            };

            _recommendationQuery.GetRecommendationsViewDto("s1", "NonMatching")
                .Returns((RecommendationsViewDto?)null);

            var component = CreateComponent();

            var method = component.GetType()
                .GetMethod("GetCategorySectionRecommendationDto", BindingFlags.NonPublic | BindingFlags.Instance);

            var task = (Task<CategorySectionRecommendationDto>)method!.Invoke(component,
                new object[] { section, status })!;
            var dto = await task;
            Assert.Equal("Unable to retrieve Section A recommendation", dto.NoRecommendationFoundErrorMessage);

        }
    }
}
