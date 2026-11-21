using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.ReviewAnswersViewBuilderTests;

public class ConfirmCheckAnswersTests : ReviewAnswersViewBuilderTestBase
{
    private const int UserId = 7;
    private const int UserEstablishmentId = 999;
    private const int FirstSelectedEstablishmentId = 101;
    private const int SecondSelectedEstablishmentId = 102;
    private const int FirstSubmissionId = 1001;
    private const int SecondSubmissionId = 1002;

    private readonly QuestionnaireSectionEntry _section = new()
    {
        Sys = new SystemDetails(SectionId),
        Name = SectionName,
    };

    private void ArrangeUserEstablishment(int userEstablishmentId)
    {
        _currentUserProvider.UserId.Returns(UserId);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(userEstablishmentId).Build());
    }

    private void ArrangeSelectedSchools(params int[] establishmentIds)
    {
        _currentUserProvider.CanSelectSchools.Returns(true);
        _currentUserProvider
            .GetSelectedSchools()
            .Returns(
                establishmentIds
                    .Select(establishmentId =>
                        new SelectedEstablishmentDataModelBuilder()
                            .WithEstablishmentId(establishmentId)
                            .Build()
                    )
                    .ToList()
            );
    }

    private void ArrangeInProgressSubmission(int establishmentId, int submissionId) =>
        _submissionService
            .GetLatestSubmissionResponsesModel(
                establishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns(new SubmissionResponsesModel(submissionId, []));

    [Fact]
    public async Task ConfirmCheckAnswers_WhenUserCannotSelectSchools_ConfirmsFocusedEstablishmentSubmission()
    {
        // Arrange
        var controller = new TestController();
        ArrangeFocusedEstablishment();
        ArrangeUserEstablishment(FocusedEstablishmentId);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        ArrangeInProgressSubmission(FocusedEstablishmentId, FirstSubmissionId);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.ConfirmCheckAnswers(
            controller,
            CategorySlug,
            SectionSlug,
            SectionName
        );

        // Assert
        await _submissionService
            .Received(1)
            .ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
                FocusedEstablishmentId,
                null,
                FirstSubmissionId,
                UserId,
                _section
            );
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(
            nameof(ReviewAnswersController.GetSchoolSelfAssessmentSummary),
            redirect.ActionName
        );
        Assert.Equal("ReviewAnswers", redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues?["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues?["sectionSlug"]);
        Assert.Equal(SectionName, controller.TempData[StatePassingMechanismConstants.SectionName]);
    }

    [Fact]
    public async Task ConfirmCheckAnswers_WhenFocusedEstablishmentIsNotTheUsersOwn_PassesUserEstablishmentAsGroupEstablishment()
    {
        // Arrange
        ArrangeFocusedEstablishment();
        ArrangeUserEstablishment(UserEstablishmentId);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        ArrangeInProgressSubmission(FocusedEstablishmentId, FirstSubmissionId);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.ConfirmCheckAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SectionName
        );

        // Assert
        await _submissionService
            .Received(1)
            .ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
                FocusedEstablishmentId,
                UserEstablishmentId,
                FirstSubmissionId,
                UserId,
                _section
            );
    }

    [Fact]
    public async Task ConfirmCheckAnswers_WhenUserCanSelectSchools_ConfirmsEachSelectedSchoolSubmission()
    {
        // Arrange
        ArrangeSelectedSchools(FirstSelectedEstablishmentId, SecondSelectedEstablishmentId);
        ArrangeUserEstablishment(UserEstablishmentId);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        ArrangeInProgressSubmission(FirstSelectedEstablishmentId, FirstSubmissionId);
        ArrangeInProgressSubmission(SecondSelectedEstablishmentId, SecondSubmissionId);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.ConfirmCheckAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SectionName
        );

        // Assert
        await _submissionService
            .Received(1)
            .ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
                FirstSelectedEstablishmentId,
                UserEstablishmentId,
                FirstSubmissionId,
                UserId,
                _section
            );
        await _submissionService
            .Received(1)
            .ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
                SecondSelectedEstablishmentId,
                UserEstablishmentId,
                SecondSubmissionId,
                UserId,
                _section
            );
        await _currentUserProvider.DidNotReceive().GetFocusedEstablishmentAsync();
    }

    [Fact]
    public async Task ConfirmCheckAnswers_WhenSelectedSchoolIsTheUsersOwnEstablishment_PassesNullGroupEstablishment()
    {
        // Arrange
        ArrangeSelectedSchools(UserEstablishmentId, SecondSelectedEstablishmentId);
        ArrangeUserEstablishment(UserEstablishmentId);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        ArrangeInProgressSubmission(UserEstablishmentId, FirstSubmissionId);
        ArrangeInProgressSubmission(SecondSelectedEstablishmentId, SecondSubmissionId);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.ConfirmCheckAnswers(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SectionName
        );

        // Assert
        await _submissionService
            .Received(1)
            .ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
                UserEstablishmentId,
                null,
                FirstSubmissionId,
                UserId,
                _section
            );
        await _submissionService
            .Received(1)
            .ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
                SecondSelectedEstablishmentId,
                UserEstablishmentId,
                SecondSubmissionId,
                UserId,
                _section
            );
    }

    [Fact]
    public async Task ConfirmCheckAnswers_WhenUserIsMat_RedirectsToTrustSelfAssessmentSummary()
    {
        // Arrange
        var controller = new TestController();
        _currentUserProvider.IsMat.Returns(true);
        ArrangeSelectedSchools(FirstSelectedEstablishmentId);
        ArrangeUserEstablishment(UserEstablishmentId);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        ArrangeInProgressSubmission(FirstSelectedEstablishmentId, FirstSubmissionId);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.ConfirmCheckAnswers(
            controller,
            CategorySlug,
            SectionSlug,
            SectionName
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(
            nameof(ReviewAnswersController.GetTrustSelfAssessmentSummary),
            redirect.ActionName
        );
        Assert.Equal("ReviewAnswers", redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues?["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues?["sectionSlug"]);
        Assert.Equal(SectionName, controller.TempData[StatePassingMechanismConstants.SectionName]);
    }

    [Fact]
    public async Task ConfirmCheckAnswers_WhenUserCanSelectSchoolsButNoneSelected_LogsErrorAndRedirectsToCheckAnswers()
    {
        // Arrange
        var controller = new TestController();
        ArrangeSelectedSchools();
        ArrangeUserEstablishment(UserEstablishmentId);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.ConfirmCheckAnswers(
            controller,
            CategorySlug,
            SectionSlug,
            SectionName
        );

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        var loggedException = Assert.IsType<InvalidOperationException>(logRecord.Exception);
        Assert.Equal(
            "User attempted to confirm answers but does not have any schools selected",
            loggedException.Message
        );
        AssertRedirectedToCheckAnswersWithError(controller, result);
        await _submissionService
            .DidNotReceiveWithAnyArgs()
            .ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
                default,
                default,
                default,
                default,
                default!
            );
    }

    [Fact]
    public async Task ConfirmCheckAnswers_WhenSectionIsNotFound_LogsErrorAndRedirectsToCheckAnswers()
    {
        // Arrange
        var controller = new TestController();
        ArrangeFocusedEstablishment();
        ArrangeUserEstablishment(FocusedEstablishmentId);
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug)
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.ConfirmCheckAnswers(
            controller,
            CategorySlug,
            SectionSlug,
            SectionName
        );

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Equal(
            $"Could not find section for slug {SectionSlug}",
            logRecord.Exception?.Message
        );
        AssertRedirectedToCheckAnswersWithError(controller, result);
    }

    [Fact]
    public async Task ConfirmCheckAnswers_WhenNoInProgressSubmissionExists_LogsErrorAndRedirectsToCheckAnswers()
    {
        // Arrange
        var controller = new TestController();
        ArrangeFocusedEstablishment();
        ArrangeUserEstablishment(FocusedEstablishmentId);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        _submissionService
            .GetLatestSubmissionResponsesModel(
                FocusedEstablishmentId,
                _section,
                SubmissionStatus.InProgress
            )
            .Returns((SubmissionResponsesModel?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.ConfirmCheckAnswers(
            controller,
            CategorySlug,
            SectionSlug,
            SectionName
        );

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Equal(
            $"Could not find an in-progress submission for establishment {FocusedEstablishmentId}",
            logRecord.Exception?.Message
        );
        AssertRedirectedToCheckAnswersWithError(controller, result);
    }

    [Fact]
    public async Task ConfirmCheckAnswers_WhenConfirmationFails_LogsErrorWithSubmissionIdAndRedirectsToCheckAnswers()
    {
        // Arrange
        var controller = new TestController();
        var exception = new InvalidOperationException("confirmation-failed");
        ArrangeFocusedEstablishment();
        ArrangeUserEstablishment(FocusedEstablishmentId);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
        ArrangeInProgressSubmission(FocusedEstablishmentId, FirstSubmissionId);
        _submissionService
            .ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
                FocusedEstablishmentId,
                null,
                FirstSubmissionId,
                UserId,
                _section
            )
            .ThrowsAsync(exception);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.ConfirmCheckAnswers(
            controller,
            CategorySlug,
            SectionSlug,
            SectionName
        );

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains(FirstSubmissionId.ToString(), logRecord.Message);
        Assert.Same(exception, logRecord.Exception);
        AssertRedirectedToCheckAnswersWithError(controller, result);
    }

    private static void AssertRedirectedToCheckAnswersWithError(
        Controller controller,
        IActionResult result
    )
    {
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ReviewAnswersController.CheckAnswers), redirect.ActionName);
        Assert.Equal("ReviewAnswers", redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues?["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues?["sectionSlug"]);
        Assert.Equal(
            ReviewAnswersViewBuilder.InlineRecommendationUnavailableErrorMessage,
            controller.TempData[StatePassingMechanismConstants.ErrorMessage]
        );
        Assert.False(controller.TempData.ContainsKey(StatePassingMechanismConstants.SectionName));
    }
}
