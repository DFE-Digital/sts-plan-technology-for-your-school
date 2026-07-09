using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Validators.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class GroupsControllerTests
    {
        private readonly ILogger<GroupsController> _logger;
        private readonly IGroupsViewBuilder _viewBuilder;
        private readonly ICurrentUserProvider _currentUser;
        private readonly GroupsController _controller;
        private readonly IGroupSelectSchoolsToAssessValidator _validator;

        public GroupsControllerTests()
        {
            _logger = Substitute.For<ILogger<GroupsController>>();
            _viewBuilder = Substitute.For<IGroupsViewBuilder>();
            _currentUser = Substitute.For<ICurrentUserProvider>();
            _validator = Substitute.For<IGroupSelectSchoolsToAssessValidator>();
            _controller = new GroupsController(_logger, _currentUser, _viewBuilder, _validator)
            {
                ControllerContext = new ControllerContext
                {
                    RouteData = new RouteData()
                }
            };
        }

        [Fact]
        public void Constructor_WithNullCurrentUser_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GroupsController(_logger, null!, _viewBuilder, _validator)
            );

            Assert.Equal("currentUser", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullViewBuilder_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GroupsController(_logger, _currentUser, null!, _validator)
            );

            Assert.Equal("groupsViewBuilder", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullValidator_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new GroupsController(_logger, _currentUser, _viewBuilder, null!)
            );

            Assert.Equal("validator", ex.ParamName);
        }

        [Fact]
        public async Task GetSelectASchoolView_CallsViewBuilderAndReturnsResult()
        {
            _viewBuilder.RouteToSelectASchoolViewModelAsync(_controller).Returns(new OkResult());

            var result = await _controller.GetSelectASchoolView();

            await _viewBuilder.Received(1).RouteToSelectASchoolViewModelAsync(_controller);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SelectSchool_RecordsSelectionAndRedirects()
        {
            var schoolUrn = "123456";
            var schoolName = "Test School";

            var controller = new GroupsController(_logger, _currentUser, _viewBuilder, _validator);

            var result = await controller.SelectSchool(schoolUrn, schoolName);

            await _viewBuilder.Received(1).RecordGroupSelectionAsync(schoolUrn, schoolName);
            _currentUser.Received(1).SetGroupSelectedSchool(schoolUrn, schoolName);

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal(UrlConstants.HomePage, redirect.Url);
        }

        [Fact]
        public async Task GetSelectSchoolsToAssessViewModelAsync_CallsViewBuilderAndReturnsResult()
        {
            var sectionSlug = "some-section";

            _viewBuilder
                .RouteToSelectSchoolsToAssessViewModelAsync(_controller, sectionSlug)
                .Returns(new OkResult());

            var result = await _controller.GetSelectSchoolsToAssessView(sectionSlug);

            await _viewBuilder
                .Received(1)
                .RouteToSelectSchoolsToAssessViewModelAsync(_controller, sectionSlug);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SubmitSelectedSchoolsToAssess_ThrowsArgumentNullException_NoSectionSlug()
        {
            var model = new GroupsSelectSchoolsToAssessViewModel()
            {
                Section = new QuestionnaireSectionEntry(),
                SchoolSubmissionInfo = new List<SubmissionInformationModel>(),
                SelectedSchoolsRefs = new List<string> { "000001", "000002" },
            };

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _controller.SubmitSelectedSchoolsToAssess(model, null!)
            );
        }

        [Fact]
        public async Task SubmitSelectedSchoolsToAssess_CallsValidator()
        {
            var categorySlug = "category";
            var sectionSlug = "section";
            var model = new GroupsSelectSchoolsToAssessViewModel()
            {
                Section = new QuestionnaireSectionEntry(),
                SchoolSubmissionInfo = new List<SubmissionInformationModel>(),
                SelectedSchoolsRefs = new List<string> { "000001", "000002" },
            };
            _controller.RouteData.Values["categorySlug"] = categorySlug;

            var result = await _controller.SubmitSelectedSchoolsToAssess(model, sectionSlug);

            await _validator
                .Received(1)
                .ValidateSelectionAsync(model, Arg.Any<ModelStateDictionary>());
        }

        [Theory]
        [InlineData(null, "cyber-security-processes", "900006")]
        [InlineData("cyber-security-standard", null, "900006")]
        [InlineData("cyber-security-standard", "cyber-security-processes", null)]
        public async Task ViewInProgressAnswers_WithNullValues_ThrowsArgumentNullException(
            string? categorySlug,
            string? sectionSlug,
            string? schoolUrn
        )
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _controller.ViewInProgressAnswers(categorySlug!, sectionSlug!, schoolUrn!)
            );
        }

        [Theory]
        [InlineData("", "cyber-security-processes", "900006")]
        [InlineData(" ", "cyber-security-processes", "900006")]
        [InlineData("cyber-security-standard", "", "900006")]
        [InlineData("cyber-security-standard", " ", "900006")]
        [InlineData("cyber-security-standard", "cyber-security-processes", "")]
        [InlineData("cyber-security-standard", "cyber-security-processes", " ")]
        public async Task ViewInProgressAnswers_WithEmptyValues_ThrowsArgumentException(
            string? categorySlug,
            string? sectionSlug,
            string? schoolUrn
        )
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.ViewInProgressAnswers(categorySlug!, sectionSlug!, schoolUrn!)
            );
        }

        [Fact]
        public async Task SubmitSelectedSchoolsToAssess_CallsViewBuilderGetMethod_WhenInvalidInput()
        {
            var categorySlug = "category";
            var sectionSlug = "section";
            var model = new GroupsSelectSchoolsToAssessViewModel()
            {
                Section = new QuestionnaireSectionEntry(),
                SchoolSubmissionInfo = new List<SubmissionInformationModel>(),
                SelectedSchoolsRefs = new List<string> { "000001", "000002", "all" },
            };
            _controller.RouteData.Values["categorySlug"] = categorySlug;

            _validator
                .When(x =>
                    x.ValidateSelectionAsync(
                        Arg.Any<GroupsSelectSchoolsToAssessViewModel>(),
                        Arg.Any<ModelStateDictionary>()
                    )
                )
                .Do(callInfo =>
                {
                    var modelState = callInfo.Arg<ModelStateDictionary>();

                    modelState.AddModelError("SelectedSchoolsRefs", "Error");
                });

            await _controller.SubmitSelectedSchoolsToAssess(model, sectionSlug);

            await _viewBuilder
                .Received(1)
                .RouteToSelectSchoolsToAssessViewModelAsync(_controller, sectionSlug, model);

            await _viewBuilder
                .DidNotReceive()
                .SubmitSelectedSchoolsToAssessAndRedirect(
                    _controller,
                    Arg.Any<string>(),
                    Arg.Any<GroupsSelectSchoolsToAssessViewModel>()
                );
        }

        [Fact]
        public async Task SubmitSelectedSchoolsToAssess_WhenValidInputCallsViewBuilderAndReturnsResult()
        {
            var categorySlug = "category";
            var sectionSlug = "section";
            var viewModel = new GroupsSelectSchoolsToAssessViewModel()
            {
                Section = new QuestionnaireSectionEntry(),
                SchoolSubmissionInfo = new List<SubmissionInformationModel>(),
                SelectedSchoolsRefs = ["00001", "00002"],
            };
            _controller.RouteData.Values["categorySlug"] = categorySlug;

            await _controller.SubmitSelectedSchoolsToAssess(viewModel, sectionSlug);

            await _viewBuilder
                .Received(1)
                .SubmitSelectedSchoolsToAssessAndRedirect(_controller, sectionSlug, viewModel);

            await _viewBuilder
                .DidNotReceive()
                .RouteToSelectSchoolsToAssessViewModelAsync(
                    _controller,
                    Arg.Any<string>(),
                    Arg.Any<GroupsSelectSchoolsToAssessViewModel>()
                );
        }

        [Fact]
        public async Task GetSelectASelfAssessment_CallsViewBuilderAndReturnsResult()
        {
            _viewBuilder
                .RouteToSelectASelfAssessmentViewModelAsync(_controller)
                .Returns(new OkResult());

            var result = await _controller.GetSelectASelfAssessment();

            await _viewBuilder.Received(1).RouteToSelectASelfAssessmentViewModelAsync(_controller);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ViewInProgressAnswers_CallsViewBuilderAndReturnsResult()
        {
            var categorySlug = "cyber-security-standard";
            var sectionSlug = "cyber-security-processes";
            var schoolUrn = "900006";

            _viewBuilder
                .RouteToViewInProgressAnswers(_controller, categorySlug, sectionSlug, schoolUrn)
                .Returns(new OkResult());

            var result = await _controller.ViewInProgressAnswers(
                categorySlug,
                sectionSlug,
                schoolUrn
            );

            await _viewBuilder
                .Received(1)
                .RouteToViewInProgressAnswers(_controller, categorySlug, sectionSlug, schoolUrn);

            Assert.IsType<OkResult>(result);
        }
    }
}
