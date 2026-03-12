using System.Diagnostics;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class PagesControllerTests
    {
        private readonly ILogger<PagesController> _logger = Substitute.For<
            ILogger<PagesController>
        >();
        private readonly IPagesViewBuilder _viewBuilder = Substitute.For<IPagesViewBuilder>();
        private readonly PagesController _controller;

        public PagesControllerTests()
        {
            _controller = new PagesController(_logger, _viewBuilder);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            };
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenViewBuilderIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new PagesController(_logger, null!)
            );
            Assert.Equal("pagesViewBuilder", ex.ParamName);
        }

        [Fact]
        public async Task RouteBasedOnOrganisationTypeAsync_ReturnsResultFromViewBuilder_WhenPageIsNotNull()
        {
            var page = new PageEntry();
            var expectedResult = new OkResult();
            _viewBuilder
                .RouteBasedOnOrganisationTypeAsync(_controller, page)
                .Returns(Task.FromResult<IActionResult>(expectedResult));

            var result = await _controller.GetByRoute(page);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task RouteBasedOnOrganisationTypeAsync_ThrowsContentfulDataUnavailableException_WhenPageIsNull()
        {
            _controller.HttpContext.Request.Path = "/some-path";

            var ex = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
                _controller.GetByRoute(null)
            );

            Assert.Equal("Could not find page at /some-path", ex.Message);
        }

        [Fact]
        public async Task GetStandardChecklist_ReturnsResultFromViewBuilder_WhenCategorySlugIsNotNull()
        {
            var expectedResult = new OkResult();
            var slug = "categorySlug";
            _viewBuilder
                .RouteToCategoryLandingPrintPageAsync(_controller, slug)
                .Returns(Task.FromResult<IActionResult>(expectedResult));

            var result = await _controller.GetStandardChecklist(slug);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetStandardChecklist_ThrowsContentfulDataUnavailableException_WhenCategorySlugIsEmpty()
        {
            _controller.HttpContext.Request.Path = "/some-path";

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetStandardChecklist("")
            );

            Assert.Equal(
                "The value cannot be an empty string or composed entirely of whitespace. (Parameter 'categorySlug')",
                ex.Message
            );
        }

        [Fact]
        public async Task ShareStandard_CallsViewBuilderAndReturnsResult()
        {
            var categorySlug = "cat";

            _viewBuilder
                .RouteToShareStandardPageAsync(_controller, categorySlug)
                .Returns(new OkResult());

            var result = await _controller.ShareStandard(categorySlug);

            await _viewBuilder.Received(1).RouteToShareStandardPageAsync(_controller, categorySlug);
        }

        [Fact]
        public async Task PostShareStandard_CallsViewBuilderAndReturnsResult()
        {
            var categorySlug = "cat";
            var inputModel = new ShareByEmailInputViewModel
            {
                EmailAddresses = new List<string> { "test@test.com", "hello@hello.com" },
                NameOfUser = "Drew",
                UserMessage = "Hello",
            };

            _viewBuilder
                .RouteToShareStandardPageAsync(_controller, categorySlug, inputModel)
                .Returns(new OkResult());

            var result = await _controller.PostShareStandard(categorySlug, inputModel);

            await _viewBuilder
                .Received(1)
                .RouteToShareStandardPageAsync(_controller, categorySlug, inputModel);
        }

        [Fact]
        public async Task HandleUnknownRoutes_ReturnsNotFoundPage_WhenCalled()
        {
            var model = new NotFoundViewModel { ContactLinkHref = "contactLinkHref" };
            var expectedResult = new ViewResult();
            var slug = "categorySlug/sectionSlug/not/a/valid/path";

            _viewBuilder.BuildNotFoundViewModelAsync().Returns(model);

            var result = await _controller.HandleUnknownRoutes(slug);

            Assert.Equal(expectedResult.GetType(), result.GetType());
        }

        [Fact]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            var traceId = "trace-123";
            Activity.Current = null;
            _controller.HttpContext.TraceIdentifier = traceId;

            var result = _controller.Error();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal(traceId, model.RequestId);
        }

        [Fact]
        public async Task NotFoundError_ReturnsViewWithViewModel()
        {
            var viewModel = new NotFoundViewModel();
            _viewBuilder.BuildNotFoundViewModelAsync().Returns(Task.FromResult(viewModel));

            var result = await _controller.NotFoundError();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(viewModel, viewResult.Model);
        }

        [Fact]
        public void NotifyError_ReturnsViewWithViewModel()
        {
            var viewModel = new NotifyShareResultsViewModel
            {
                ActionModel = new ActionViewModel
                {
                    ActionName = "Index",
                    ControllerName = "Home",
                    LinkText = "Home",
                },
                SendResults = [],
            };

            _viewBuilder.BuildNotifyShareResultsViewModel(_controller).Returns(viewModel);

            var result = _controller.NotifyError();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(viewModel, viewResult.Model);
        }

        [Fact]
        public void Error_ReturnsViewWithActivityId_WhenActivityIsNotNull()
        {
            var activity = new Activity("TestActivity");
            activity.Start();
            Activity.Current = activity;

            var result = _controller.Error();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal(activity.Id, model.RequestId);

            activity.Stop();
            Activity.Current = null;
        }
    }
}
