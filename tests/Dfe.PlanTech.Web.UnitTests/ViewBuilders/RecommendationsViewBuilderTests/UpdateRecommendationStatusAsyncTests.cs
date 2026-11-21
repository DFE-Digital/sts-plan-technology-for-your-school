using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests;

public class UpdateRecommendationStatusAsyncTests : RecommendationsViewBuilderTestBase
{
    private const int SectionIncludeLevel = 2;
    private const string DefaultNote = "Status changed to Complete";

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            null,
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.UpdateRecommendationStatusAsync(
                new TestController(),
                CategorySlug,
                SectionSlug,
                SecondChunkSlug,
                new SingleRecommendationInputViewModel { SelectedStatus = "Complete" }
            )
        );
        Assert.Equal($"Could not find section for slug {SectionSlug}", exception.Message);
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenChunkIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.UpdateRecommendationStatusAsync(
                new TestController(),
                CategorySlug,
                SectionSlug,
                "missing-chunk",
                new SingleRecommendationInputViewModel { SelectedStatus = "Complete" }
            )
        );
        Assert.Equal(
            "No recommendation chunk found with slug matching: missing-chunk",
            exception.Message
        );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenStatusIsUnchangedAndNoNotesProvided_ReturnsSingleRecommendationViewWithoutUpdating()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _recommendationService
            .GetRecommendationHistoryAsync("chunk-2", FocusedEstablishmentId)
            .Returns([
                new SqlEstablishmentRecommendationHistoryDtoBuilder()
                    .WithNewStatus(RecommendationStatus.Complete)
                    .Build(),
            ]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.UpdateRecommendationStatusAsync(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            new SingleRecommendationInputViewModel { SelectedStatus = "Complete", Notes = " " }
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("SingleRecommendation", view.ViewName);
        await _recommendationService
            .DidNotReceiveWithAnyArgs()
            .UpdateRecommendationStatusAsync(default!, default, default, default, default, default);
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenStatusIsNotRecognisedAndNoHistoryExists_ReturnsSingleRecommendationViewWithoutUpdating()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _recommendationService
            .GetRecommendationHistoryAsync("chunk-2", FocusedEstablishmentId)
            .Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.UpdateRecommendationStatusAsync(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            new SingleRecommendationInputViewModel { SelectedStatus = "unrecognised-status" }
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("SingleRecommendation", view.ViewName);
        await _recommendationService
            .DidNotReceiveWithAnyArgs()
            .UpdateRecommendationStatusAsync(default!, default, default, default, default, default);
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenStatusChangesForSchoolUser_UpdatesStatusWithDefaultNoteAndNoMatEstablishment()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        ArrangeStatusChangeToComplete();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateRecommendationStatusAsync(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            new SingleRecommendationInputViewModel { SelectedStatus = "Complete" }
        );

        // Assert
        await _recommendationService
            .Received(1)
            .UpdateRecommendationStatusAsync(
                "chunk-2",
                FocusedEstablishmentId,
                CurrentUserId,
                RecommendationStatus.Complete,
                DefaultNote,
                null
            );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenStatusChanges_SetsSuccessTitleAndRedirectsToSingleRecommendation()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        ArrangeStatusChangeToComplete();
        _microcopyProvider
            .GetTextByKeyAsync(
                ContentfulMicrocopyConstants.SingleRecommendationSuccessHeader,
                Arg.Is<Dictionary<string, string>?>(values =>
                    values != null && values["recStatus"] == "Complete"
                )
            )
            .Returns("Status updated to Complete");
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.UpdateRecommendationStatusAsync(
            controller,
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            new SingleRecommendationInputViewModel { SelectedStatus = "Complete" }
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetSingleRecommendation", redirect.ActionName);
        Assert.Equal("Recommendations", redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues!["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
        Assert.Equal(SecondChunkSlug, redirect.RouteValues["chunkSlug"]);
        Assert.Equal(
            "Status updated to Complete",
            controller.TempData[StatePassingMechanismConstants.StatusUpdateSuccessTitle]
        );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenNotesAreProvided_UpdatesStatusWithProvidedNotes()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        ArrangeStatusChangeToComplete();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateRecommendationStatusAsync(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            new SingleRecommendationInputViewModel
            {
                SelectedStatus = "Complete",
                Notes = "Finished the migration",
            }
        );

        // Assert
        await _recommendationService
            .Received(1)
            .UpdateRecommendationStatusAsync(
                "chunk-2",
                FocusedEstablishmentId,
                CurrentUserId,
                RecommendationStatus.Complete,
                "Finished the migration",
                null
            );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenDefaultNoteIsBlank_UpdatesStatusWithoutNote()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        ArrangeStatusChangeToComplete();
        _microcopyProvider
            .GetTextByKeyAsync(
                ContentfulMicrocopyConstants.SingleRecommendationHistoryReason,
                Arg.Is<Dictionary<string, string>?>(values =>
                    values != null && values["recStatus"] == "Complete"
                )
            )
            .Returns("  ");
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateRecommendationStatusAsync(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            new SingleRecommendationInputViewModel { SelectedStatus = "Complete" }
        );

        // Assert
        await _recommendationService
            .Received(1)
            .UpdateRecommendationStatusAsync(
                "chunk-2",
                FocusedEstablishmentId,
                CurrentUserId,
                RecommendationStatus.Complete,
                null,
                null
            );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenStatusIsUnchangedButNotesProvided_UpdatesStatusWithoutStatusDynamicValues()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _currentUserProvider.UserId.Returns(CurrentUserId);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(UserEstablishmentId).Build());
        _recommendationService
            .GetRecommendationHistoryAsync("chunk-2", FocusedEstablishmentId)
            .Returns([
                new SqlEstablishmentRecommendationHistoryDtoBuilder()
                    .WithNewStatus(RecommendationStatus.Complete)
                    .Build(),
            ]);
        _microcopyProvider
            .GetTextByKeyAsync(ContentfulMicrocopyConstants.SingleRecommendationSuccessHeader, null)
            .Returns("Notes updated");
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateRecommendationStatusAsync(
            controller,
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            new SingleRecommendationInputViewModel
            {
                SelectedStatus = "Complete",
                Notes = "Added more detail",
            }
        );

        // Assert
        await _recommendationService
            .Received(1)
            .UpdateRecommendationStatusAsync(
                "chunk-2",
                FocusedEstablishmentId,
                CurrentUserId,
                RecommendationStatus.Complete,
                "Added more detail",
                null
            );
        Assert.Equal(
            "Notes updated",
            controller.TempData[StatePassingMechanismConstants.StatusUpdateSuccessTitle]
        );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenUserIsMat_UpdatesStatusWithUserEstablishmentAsMatEstablishment()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        ArrangeStatusChangeToComplete();
        _currentUserProvider.IsMat.Returns(true);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateRecommendationStatusAsync(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            new SingleRecommendationInputViewModel { SelectedStatus = "Complete" }
        );

        // Assert
        await _recommendationService
            .Received(1)
            .UpdateRecommendationStatusAsync(
                "chunk-2",
                FocusedEstablishmentId,
                CurrentUserId,
                RecommendationStatus.Complete,
                DefaultNote,
                UserEstablishmentId
            );
    }

    [Fact]
    public async Task UpdateRecommendationStatusAsync_WhenUserIsSatOrSSat_UpdatesStatusWithUserEstablishmentAsMatEstablishment()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build(),
            SectionIncludeLevel
        );
        ArrangeStatusChangeToComplete();
        _currentUserProvider.IsSatOrSSat.Returns(true);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        await systemUnderTest.UpdateRecommendationStatusAsync(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            new SingleRecommendationInputViewModel { SelectedStatus = "Complete" }
        );

        // Assert
        await _recommendationService
            .Received(1)
            .UpdateRecommendationStatusAsync(
                "chunk-2",
                FocusedEstablishmentId,
                CurrentUserId,
                RecommendationStatus.Complete,
                DefaultNote,
                UserEstablishmentId
            );
    }

    private void ArrangeStatusChangeToComplete()
    {
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _currentUserProvider.UserId.Returns(CurrentUserId);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithId(UserEstablishmentId).Build());
        _recommendationService
            .GetRecommendationHistoryAsync("chunk-2", FocusedEstablishmentId)
            .Returns([
                new SqlEstablishmentRecommendationHistoryDtoBuilder()
                    .WithNewStatus(RecommendationStatus.InProgress)
                    .Build(),
            ]);
        _microcopyProvider
            .GetTextByKeyAsync(
                ContentfulMicrocopyConstants.SingleRecommendationHistoryReason,
                Arg.Is<Dictionary<string, string>?>(values =>
                    values != null && values["recStatus"] == "Complete"
                )
            )
            .Returns(DefaultNote);
    }
}
