using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Validators.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Amqp.Transaction;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.ComponentModel.DataAnnotations;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class GroupsControllerTests
    {
        private readonly ILogger<GroupsController> _logger;
        private readonly IGroupsViewBuilder _viewBuilder;
        private readonly ICurrentUser _currentUser;
        private readonly IGroupSelectSchoolsToAssessValidator _validator;
        private readonly GroupsController _controller;

        public GroupsControllerTests()
        {
            _logger = Substitute.For<ILogger<GroupsController>>();
            _viewBuilder = Substitute.For<IGroupsViewBuilder>();
            _currentUser = Substitute.For<ICurrentUser>();
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

            _viewBuilder.RouteToSelectSchoolsToAssessViewModelAsync(_controller, sectionSlug).Returns(new OkResult());

            var result = await _controller.GetSelectSchoolsToAssessView(sectionSlug);

            await _viewBuilder.Received(1).RouteToSelectSchoolsToAssessViewModelAsync(_controller, sectionSlug);
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SubmitSelectedSchoolsToAssess_ThrowsArgumentNullException_NoSectionSlug()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _controller.SubmitSelectedSchoolsToAssess(new GroupSelectSchoolsToAssessInputViewModel(), null!));
        }

        [Fact]
        public async Task SubmitSelectedSchoolsToAssess_CallsValidator()
        {
            var categorySlug = "category";
            var sectionSlug = "section";
            var schoolRefs = new List<string> { "000001", "000002" };
            var inputViewModel = new GroupSelectSchoolsToAssessInputViewModel();
            _controller.RouteData.Values["categorySlug"] = categorySlug;

            var result = await _controller.SubmitSelectedSchoolsToAssess(inputViewModel, sectionSlug);

            await _validator.Received(1).ValidateSelectionAsync(inputViewModel, Arg.Any<ModelStateDictionary>());
        }

        [Fact]
        public async Task SubmitSelectedSchoolsToAssess_CallsViewBuilderGetMethod_WhenInvalidInput()
        {
            var categorySlug = "category";
            var sectionSlug = "section";
            var schoolRefs = new List<string> { "000001", "000002" };
            var inputViewModel = new GroupSelectSchoolsToAssessInputViewModel();
            _controller.RouteData.Values["categorySlug"] = categorySlug;


            _validator
                .When(x => x.ValidateSelectionAsync(
                    Arg.Any<GroupSelectSchoolsToAssessInputViewModel>(),
                    Arg.Any<ModelStateDictionary>()))
                .Do(callInfo =>
                {
                    var modelState =
                        callInfo.Arg<ModelStateDictionary>();

                    modelState.AddModelError(
                        "SelectedSchoolsRefs",
                        "Error");
                });

            await _controller.SubmitSelectedSchoolsToAssess(inputViewModel, sectionSlug);

            await _viewBuilder.Received(1)
                .RouteToSelectSchoolsToAssessViewModelAsync(
                    _controller,
                    sectionSlug,
                    inputViewModel);


            await _viewBuilder.DidNotReceive()
                .SubmitSelectedSchoolsToAssessAndRedirect(
                    _controller,
                    Arg.Any<string>(),
                    Arg.Any<GroupSelectSchoolsToAssessInputViewModel>());

        }

        [Fact]
        public async Task SubmitSelectedSchoolsToAssess_WhenValidInputCallsViewBuilderAndReturnsResult()
        {
            var categorySlug = "category";
            var sectionSlug = "section";
            var inputViewModel = new GroupSelectSchoolsToAssessInputViewModel() { SelectedSchoolsRefs = [ "00001", "00002" ] };
            _controller.RouteData.Values["categorySlug"] = categorySlug;

            await _controller.SubmitSelectedSchoolsToAssess(inputViewModel, sectionSlug);

            await _viewBuilder.Received(1).SubmitSelectedSchoolsToAssessAndRedirect(_controller, sectionSlug, inputViewModel);

            await _viewBuilder.DidNotReceive()
                .RouteToSelectSchoolsToAssessViewModelAsync(
                    _controller,
                    Arg.Any<string>(),
                    Arg.Any<GroupSelectSchoolsToAssessInputViewModel>());
        }
    }
}
