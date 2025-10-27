using System.Diagnostics;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.Tests.Controllers
{
    public class PagesControllerTests
    {
        private readonly ILogger<PagesController> _logger = Substitute.For<ILogger<PagesController>>();
        private readonly ICategoryLandingViewComponentViewBuilder _categoryLandingViewComponentBuilder = Substitute.For<ICategoryLandingViewComponentViewBuilder>();
        private readonly IPagesViewBuilder _pagesViewBuilder = Substitute.For<IPagesViewBuilder>();
        private readonly PagesController _controller;

        public PagesControllerTests()
        {
            _controller = new PagesController(_logger, _categoryLandingViewComponentBuilder, _pagesViewBuilder);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenViewBuilderIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new PagesController(_logger, _categoryLandingViewComponentBuilder, null!)
            );
            Assert.Equal("pagesViewBuilder", ex.ParamName);
        }

        [Fact]
        public async Task GetByRoute_ReturnsResultFromViewBuilder_WhenPageIsNotNull()
        {
            var page = new PageEntry();
            var expectedResult = new OkResult();
            _pagesViewBuilder.RouteBasedOnOrganisationTypeAsync(_controller, page)
                .Returns(Task.FromResult<IActionResult>(expectedResult));

            var result = await _controller.GetByRoute(page);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task GetByRoute_ThrowsContentfulDataUnavailableException_WhenPageIsNull()
        {
            _controller.HttpContext.Request.Path = "/some-path";

            var ex = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
                _controller.GetByRoute(null)
            );

            Assert.Equal("Could not find page at /some-path", ex.Message);
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
            _pagesViewBuilder.BuildNotFoundViewModel().Returns(Task.FromResult(viewModel));

            var result = await _controller.NotFoundError();

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
