using System.Text.Json;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Infrastructure.ServiceBus.Interfaces;
using Dfe.PlanTech.Infrastructure.ServiceBus.Queueing;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders.Interfaces;
using Dfe.PlanTech.Web.ViewModels.QaVisualiser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.Tests.Controllers
{
    public class CmsControllerTests
    {
        private readonly ILogger<CmsController> _logger = Substitute.For<ILogger<CmsController>>();
        private readonly CmsController _controller;
        private readonly ICmsViewBuilder _viewBuilder = Substitute.For<ICmsViewBuilder>();

        public CmsControllerTests()
        {
            _controller = new CmsController(_logger, _viewBuilder);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task WebhookPayload_ReturnsOk_WhenWriteSucceeds()
        {
            var json = JsonDocument.Parse("{}");
            var writeToQueueCommand = Substitute.For<IWriteCmsWebhookToQueueCommand>();
            writeToQueueCommand.WriteMessageToQueue(json, _controller.HttpContext.Request)
                .Returns(Task.FromResult(new QueueWriteResult(true)));

            var result = await _controller.WebhookPayload(json, writeToQueueCommand);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task WebhookPayload_ReturnsBadRequest_WhenWriteFails()
        {
            var json = JsonDocument.Parse("{}");
            var writeToQueueCommand = Substitute.For<IWriteCmsWebhookToQueueCommand>();
            var failureResult = new QueueWriteResult(false);
            writeToQueueCommand.WriteMessageToQueue(json, _controller.HttpContext.Request)
                .Returns(Task.FromResult(failureResult));

            var result = await _controller.WebhookPayload(json, writeToQueueCommand);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(failureResult, badRequest.Value);
        }

        [Fact]
        public async Task WebhookPayload_ReturnsBadRequest_OnException()
        {
            // Arrange
            var json = JsonDocument.Parse("{}");
            var writeToQueueCommand = Substitute.For<IWriteCmsWebhookToQueueCommand>();
            writeToQueueCommand.WriteMessageToQueue(json, _controller.HttpContext.Request)
                .Throws(new Exception("Queue error"));

            var result = await _controller.WebhookPayload(json, writeToQueueCommand);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Queue error", badRequest.Value);
        }

        [Fact]
        public async Task GetSections_ReturnsSections()
        {
            var sections = new List<SectionViewModel>
            {
                new SectionViewModel(new QuestionnaireSectionEntry{ Name = "test-section 1", Sys = new SystemDetails { Id = "1" } }),
                new SectionViewModel(new QuestionnaireSectionEntry{ Name = "test-section 2", Sys = new SystemDetails { Id = "2" } })
            };

            _viewBuilder.GetAllSectionsAsync().Returns(Task.FromResult<IEnumerable<SectionViewModel>>(sections));

            var result = await _controller.GetSections();

            Assert.Equal(sections, result);
        }

        [Fact]
        public async Task GetChunks_ReturnsResultFromViewBuilder()
        {
            var page = 1;
            var expectedResult = new OkResult();
            _viewBuilder.GetChunks(_controller, page).Returns(Task.FromResult<IActionResult>(expectedResult));

            var result = await _controller.GetChunks(page);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenViewBuilderIsNull()
        {
            var logger = Substitute.For<ILogger<CmsController>>();
            ICmsViewBuilder? viewBuilder = null;

            var exception = Assert.Throws<ArgumentNullException>(() =>
                new CmsController(logger, viewBuilder!)
            );

            Assert.Equal("viewBuilder", exception.ParamName);
        }
    }
}
