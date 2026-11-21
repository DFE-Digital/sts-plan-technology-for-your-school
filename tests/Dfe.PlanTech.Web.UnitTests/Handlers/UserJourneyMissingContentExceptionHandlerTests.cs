using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Handlers;

public class UserJourneyMissingContentExceptionHandlerTests
{
    private const int FocusedEstablishmentId = 456;
    private const string SectionId = "section-id";
    private const string SectionName = "Section name";
    private const string ConfiguredErrorMessage =
        "Please try again - your answers may be out of date.";

    private readonly FakeLogger<UserJourneyMissingContentExceptionHandler> _logger = new();
    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                {
                    UserJourneyMissingContentExceptionHandler.ErrorMessageConfigKey,
                    ConfiguredErrorMessage
                },
            }
        )
        .Build();
    private readonly ISubmissionService _submissionService = Substitute.For<ISubmissionService>();
    private readonly ICurrentUserProvider _currentUserProvider =
        Substitute.For<ICurrentUserProvider>();

    private UserJourneyMissingContentExceptionHandler CreateSystemUnderTest() =>
        new(_logger, _configuration, _submissionService, _currentUserProvider);

    private static UserJourneyMissingContentException BuildException() =>
        new(
            "Missing content",
            new QuestionnaireSectionEntry { Sys = new SystemDetails(SectionId), Name = SectionName }
        );

    [Fact]
    public void Constructor_WhenAnyDependencyIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        // The logger is not null-guarded in production, so it is excluded here.
        Assert.Throws<ArgumentNullException>("configuration", () =>
            new UserJourneyMissingContentExceptionHandler(
                _logger,
                null!,
                _submissionService,
                _currentUserProvider
            )
        );
        Assert.Throws<ArgumentNullException>("submissionService", () =>
            new UserJourneyMissingContentExceptionHandler(
                _logger,
                _configuration,
                null!,
                _currentUserProvider
            )
        );
        Assert.Throws<ArgumentNullException>("currentUser", () =>
            new UserJourneyMissingContentExceptionHandler(
                _logger,
                _configuration,
                _submissionService,
                null!
            )
        );
    }

    [Fact]
    public async Task Handle_WhenCalled_MarksSectionSubmissionInaccessibleForFocusedEstablishment()
    {
        // Arrange
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .Build()
            );
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.Handle(controller, BuildException());

        // Assert
        await _submissionService
            .Received(1)
            .SetSubmissionInaccessibleAsync(FocusedEstablishmentId, SectionId);
    }

    [Fact]
    public async Task Handle_WhenCalled_SetsConfiguredErrorMessageInTempData()
    {
        // Arrange
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .Build()
            );
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.Handle(controller, BuildException());

        // Assert
        Assert.Equal(
            ConfiguredErrorMessage,
            controller.TempData[UserJourneyMissingContentExceptionHandler.ErrorMessageTempDataKey]
        );
    }

    [Fact]
    public async Task Handle_WhenCalled_RedirectsToHomePage()
    {
        // Arrange
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .Build()
            );
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.Handle(controller, BuildException());

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.Equal(nameof(PagesController).GetControllerNameSlug(), redirect.ControllerName);
        Assert.Equal(UrlConstants.Home, redirect.RouteValues!["route"]);
    }

    [Fact]
    public async Task Handle_WhenCalled_LogsErrorWithSectionName()
    {
        // Arrange
        _currentUserProvider
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .Build()
            );
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.Handle(controller, BuildException());

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains(SectionName, logRecord.Message);
    }
}
