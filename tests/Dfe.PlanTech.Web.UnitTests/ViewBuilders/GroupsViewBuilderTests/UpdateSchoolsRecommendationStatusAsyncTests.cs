using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.GroupsViewBuilderTests;

public class UpdateSchoolsRecommendationStatusAsyncTests : GroupsViewBuilderTestBase
{
    private const int UserId = 7;
    private const int TrustEstablishmentId = 500;
    private const string Notes = "Reviewed on site";

    private readonly TestController _controller = new();

    private readonly GroupRecommendationInputViewModel _inputViewModel = new()
    {
        SelectedSchoolsRefs = ["100011", "100012"],
        SelectedStatus = "Complete",
        Notes = Notes,
    };

    public UpdateSchoolsRecommendationStatusAsyncTests()
    {
        _controller.RouteData.Values["categorySlug"] = CategorySlug;
        _currentUserProvider.UserId.Returns(UserId);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(TrustEstablishmentId).Build());
        _currentUserProvider.GetSelectedSchoolEstablishmentIds().Returns([11, 12]);
        _contentfulService.GetSectionBySlugAsync(SectionSlug).Returns(_section);
    }

    [Fact]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenCategorySlugIsMissingFromRoute_ThrowsInvalidDataException()
    {
        // Arrange
        _controller.RouteData.Values.Remove("categorySlug");
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
                _controller,
                SectionSlug,
                RecommendationSlug,
                _inputViewModel
            )
        );
        Assert.Equal("Missing category slug", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-status")]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenStatusIsNotRecognised_ThrowsArgumentException(
        string? selectedStatus
    )
    {
        // Arrange
        _inputViewModel.SelectedStatus = selectedStatus;
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
                _controller,
                SectionSlug,
                RecommendationSlug,
                _inputViewModel
            )
        );
        Assert.Equal("Invalid status selected.", exception.Message);
        await _recommendationService
            .DidNotReceiveWithAnyArgs()
            .UpdateEstablishmentsRecommendationStatusAsync(default!, default!, default, default);
    }

    [Fact]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenSectionIsMissing_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        _contentfulService
            .GetSectionBySlugAsync(SectionSlug)
            .Returns((QuestionnaireSectionEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
                _controller,
                SectionSlug,
                RecommendationSlug,
                _inputViewModel
            )
        );
        Assert.Contains(SectionSlug, exception.Message);
    }

    [Fact]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenRecommendationSlugIsNotInSection_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
                _controller,
                SectionSlug,
                "unknown-recommendation-slug",
                _inputViewModel
            )
        );
        Assert.Contains("unknown-recommendation-slug", exception.Message);
    }

    [Fact]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenCalled_UpdatesStatusForSelectedSchools()
    {
        // Arrange
        ArrangeSuccessMessage("2", "2 schools updated");
        int[] expectedEstablishmentIds = [11, 12];
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
            _controller,
            SectionSlug,
            RecommendationSlug,
            _inputViewModel
        );

        // Assert
        await _recommendationService
            .Received(1)
            .UpdateEstablishmentsRecommendationStatusAsync(
                RecommendationChunkId,
                Arg.Is<IEnumerable<int>>(ids => ids.SequenceEqual(expectedEstablishmentIds)),
                UserId,
                RecommendationStatus.Complete,
                Notes,
                TrustEstablishmentId
            );
    }

    [Fact]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenUpdateSucceeds_RedirectsToMatSingleRecommendation()
    {
        // Arrange
        ArrangeSuccessMessage("2", "2 schools updated");
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
            _controller,
            SectionSlug,
            RecommendationSlug,
            _inputViewModel
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(
            nameof(RecommendationsController.GetMatSingleRecommendation),
            redirect.ActionName
        );
        Assert.Equal(CategorySlug, redirect.RouteValues!["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
        Assert.Equal(RecommendationSlug, redirect.RouteValues["chunkSlug"]);
    }

    [Fact]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenSeveralSchoolsAreUpdated_SetsPluralSuccessMessage()
    {
        // Arrange
        ArrangeSuccessMessage("2", "2 schools updated");
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
            _controller,
            SectionSlug,
            RecommendationSlug,
            _inputViewModel
        );

        // Assert
        Assert.Equal(
            "2 schools updated",
            _controller.TempData[StatePassingMechanismConstants.StatusUpdateSuccessTitle]
        );
    }

    [Fact]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenOneSchoolIsUpdated_SetsSingularSuccessMessage()
    {
        // Arrange
        _currentUserProvider.GetSelectedSchoolEstablishmentIds().Returns([11]);
        ArrangeSuccessMessage("1", "1 schools updated");
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
            _controller,
            SectionSlug,
            RecommendationSlug,
            _inputViewModel
        );

        // Assert
        Assert.Equal(
            "1 school updated",
            _controller.TempData[StatePassingMechanismConstants.StatusUpdateSuccessTitle]
        );
    }

    [Fact]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenUpdateFails_LogsError()
    {
        // Arrange
        ArrangeFailedUpdateAndStatusView();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
            _controller,
            SectionSlug,
            RecommendationSlug,
            _inputViewModel
        );

        // Assert
        var logRecord = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, logRecord.Level);
        Assert.Contains("Failed to update recommendation status", logRecord.Message);
        Assert.IsType<InvalidOperationException>(logRecord.Exception);
    }

    [Fact]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenUpdateFails_ReturnsSelectStatusViewWithErrorMessage()
    {
        // Arrange
        ArrangeFailedUpdateAndStatusView();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
            _controller,
            SectionSlug,
            RecommendationSlug,
            _inputViewModel
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("GroupsSelectStatusToUpdate", view.ViewName);
        var viewModel = Assert.IsType<GroupsSelectStatusToUpdateViewModel>(view.Model);
        Assert.Equal(
            "There was a problem updating the recommendation status. Try again.",
            viewModel.StatusErrorMessage
        );
        Assert.Equal(RecommendationStatus.Complete, viewModel.SelectedStatusKey);
        Assert.Equal(["100011", "100012"], viewModel.SelectedSchoolsRefs);
    }

    [Fact]
    public async Task UpdateSchoolsRecommendationStatusAsync_WhenUpdateFails_DoesNotSetSuccessMessage()
    {
        // Arrange
        ArrangeFailedUpdateAndStatusView();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateSchoolsRecommendationStatusAsync(
            _controller,
            SectionSlug,
            RecommendationSlug,
            _inputViewModel
        );

        // Assert
        Assert.False(
            _controller.TempData.ContainsKey(
                StatePassingMechanismConstants.StatusUpdateSuccessTitle
            )
        );
    }

    private void ArrangeSuccessMessage(string schoolCount, string message) =>
        _microcopyProvider
            .GetTextByKeyAsync(
                ContentfulMicrocopyConstants.GroupsUpdateStatusSuccessPanelText,
                Arg.Is<Dictionary<string, string>?>(values =>
                    values != null && values["schoolCount"] == schoolCount
                )
            )
            .Returns(message);

    private void ArrangeFailedUpdateAndStatusView()
    {
        _recommendationService
            .UpdateEstablishmentsRecommendationStatusAsync(
                RecommendationChunkId,
                Arg.Any<IEnumerable<int>>(),
                UserId,
                RecommendationStatus.Complete,
                Notes,
                TrustEstablishmentId
            )
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        _currentUserProvider
            .GetGroupSchoolsAsync()
            .Returns(
                new List<SelectedEstablishmentDataModel> { _oakfieldPrimary, _riverbankPrimary }
            );
        _establishmentService
            .GetEstablishmentsByReferencesAsync(Arg.Any<IEnumerable<string>>())
            .Returns(new List<SqlEstablishmentDto>());
    }
}
