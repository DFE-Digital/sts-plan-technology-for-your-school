using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests;

public class RouteToShareRecommendationAsyncSchoolTests : RecommendationsViewBuilderTestBase
{
    private const string ShareViewName = "~/Views/Shared/Email/ShareByEmail.cshtml";
    private const string TextBodyId = "text-body-2";
    private const string Recipient = "recipient@example.com";

    private static readonly ComponentTextBodyEntry TextBody = new()
    {
        Sys = new SystemDetails(TextBodyId),
    };

    private static ShareByEmailInputViewModel InputModel =>
        new()
        {
            NameOfUser = "Alex Morgan",
            EmailAddresses = [Recipient],
            UserMessage = "Worth a read",
        };

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenInputModelIsNull_ReturnsShareViewForSingleRecommendation()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().WithHeaderText("Networking").Build(),
            new QuestionnaireSectionEntryBuilder().Build()
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareRecommendationAsync(
            new TestController(),
            CategorySlug,
            SectionSlug,
            SecondChunkSlug
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ShareViewName, view.ViewName);
        var viewModel = Assert.IsType<ShareByEmailViewModel>(view.Model);
        Assert.Equal("Recommendations", viewModel.PostController);
        Assert.Equal("ShareSingleRecommendation", viewModel.PostAction);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Equal(SectionSlug, viewModel.SectionSlug);
        Assert.Equal(SecondChunkSlug, viewModel.ChunkSlug);
        Assert.Equal("Second chunk", viewModel.Caption);
        Assert.Equal("Share a recommendation by email", viewModel.Heading);
        Assert.Null(viewModel.InputModel);
        Assert.False(viewModel.IsMat);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenChunkSlugDiffersOnlyByCase_ReturnsShareView()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build()
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareRecommendationAsync(
            new TestController(),
            CategorySlug,
            SectionSlug,
            "SECOND-CHUNK"
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<ShareByEmailViewModel>(view.Model);
        Assert.Equal("Second chunk", viewModel.Caption);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenModelStateIsInvalid_ReturnsShareViewWithInputModel()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build()
        );
        var controller = new TestController();
        controller.ModelState.AddModelError("EmailAddresses", "Enter a valid email address");
        var inputModel = InputModel;
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareRecommendationAsync(
            controller,
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            inputModel
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ShareViewName, view.ViewName);
        var viewModel = Assert.IsType<ShareByEmailViewModel>(view.Model);
        Assert.Same(inputModel, viewModel.InputModel);
        _notifyService
            .DidNotReceiveWithAnyArgs()
            .SendSingleRecommendationEmail(
                default!,
                default!,
                default!,
                default!,
                default!,
                default
            );
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenCategoryIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(null, new QuestionnaireSectionEntryBuilder().Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToShareRecommendationAsync(
                new TestController(),
                CategorySlug,
                SectionSlug,
                SecondChunkSlug
            )
        );
        Assert.Equal($"Could not find category for slug {CategorySlug}", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenSectionIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(new QuestionnaireCategoryEntryBuilder().Build(), null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToShareRecommendationAsync(
                new TestController(),
                CategorySlug,
                SectionSlug,
                SecondChunkSlug
            )
        );
        Assert.Equal($"Could not find section for slug {SectionSlug}", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenChunkIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build()
        );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToShareRecommendationAsync(
                new TestController(),
                CategorySlug,
                SectionSlug,
                "missing-chunk"
            )
        );
        Assert.Equal("Could not find chunk for slug missing-chunk", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenTextBodyIsNotFound_ThrowsContentfulDataUnavailableException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build()
        );
        _contentfulService.GetTextBodyByIdAsync(TextBodyId).Returns((ComponentTextBodyEntry)null!);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ContentfulDataUnavailableException>(() =>
            systemUnderTest.RouteToShareRecommendationAsync(
                new TestController(),
                CategorySlug,
                SectionSlug,
                SecondChunkSlug,
                InputModel
            )
        );
        Assert.Equal($"Could not find text body entry for id {TextBodyId}", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenRecommendationHasNoHistory_ThrowsInvalidDataException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build()
        );
        _contentfulService.GetTextBodyByIdAsync(TextBodyId).Returns(TextBody);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _recommendationService
            .GetLatestRecommendationHistoryAsync(FocusedEstablishmentId, "chunk-2")
            .Returns((SqlEstablishmentRecommendationHistoryDto?)null);
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.RouteToShareRecommendationAsync(
                new TestController(),
                CategorySlug,
                SectionSlug,
                SecondChunkSlug,
                InputModel
            )
        );
        Assert.Equal("Cannot send an email without a recommendation status", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenLatestHistoryHasNoStatus_ThrowsInvalidDataException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build()
        );
        _contentfulService.GetTextBodyByIdAsync(TextBodyId).Returns(TextBody);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _recommendationService
            .GetLatestRecommendationHistoryAsync(FocusedEstablishmentId, "chunk-2")
            .Returns(
                new SqlEstablishmentRecommendationHistoryDtoBuilder().WithNewStatus(null).Build()
            );
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.RouteToShareRecommendationAsync(
                new TestController(),
                CategorySlug,
                SectionSlug,
                SecondChunkSlug,
                InputModel
            )
        );
        Assert.Equal("Cannot send an email without a recommendation status", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenEmailIsSent_RedirectsBackToSingleRecommendation()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().WithHeaderText("Networking").Build(),
            new QuestionnaireSectionEntryBuilder().Build()
        );
        _contentfulService.GetTextBodyByIdAsync(TextBodyId).Returns(TextBody);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _recommendationService
            .GetLatestRecommendationHistoryAsync(FocusedEstablishmentId, "chunk-2")
            .Returns(
                new SqlEstablishmentRecommendationHistoryDtoBuilder()
                    .WithNewStatus(RecommendationStatus.Complete)
                    .Build()
            );
        _notifyService
            .SendSingleRecommendationEmail(
                Arg.Is<ShareByEmailModel>(model =>
                    model.NameOfUser == "Alex Morgan"
                    && model.UserMessage == "Worth a read"
                    && model.EmailAddresses.SequenceEqual(new[] { Recipient })
                ),
                TextBody,
                "Springfield Primary",
                "Second chunk",
                "Networking",
                RecommendationStatus.Complete
            )
            .Returns([new NotifySendResult { Recipient = Recipient }]);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareRecommendationAsync(
            controller,
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            InputModel
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetSingleRecommendation", redirect.ActionName);
        Assert.Equal("Recommendations", redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues!["categorySlug"]);
        Assert.Equal(SectionSlug, redirect.RouteValues["sectionSlug"]);
        Assert.Equal(SecondChunkSlug, redirect.RouteValues["chunkSlug"]);
        var shareResults = Assert.IsType<string>(
            controller.TempData[StatePassingMechanismConstants.NotifyShareResults]
        );
        Assert.Contains(Recipient, shareResults);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenEmailSendingFails_RedirectsToNotifyError()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().WithHeaderText("Networking").Build(),
            new QuestionnaireSectionEntryBuilder().Build()
        );
        _contentfulService.GetTextBodyByIdAsync(TextBodyId).Returns(TextBody);
        _currentUserProvider.GetFocusedEstablishmentAsync().Returns(FocusedEstablishment);
        _recommendationService
            .GetLatestRecommendationHistoryAsync(FocusedEstablishmentId, "chunk-2")
            .Returns(
                new SqlEstablishmentRecommendationHistoryDtoBuilder()
                    .WithNewStatus(RecommendationStatus.Complete)
                    .Build()
            );
        _notifyService
            .SendSingleRecommendationEmail(
                Arg.Is<ShareByEmailModel>(model => model.NameOfUser == "Alex Morgan"),
                TextBody,
                "Springfield Primary",
                "Second chunk",
                "Networking",
                RecommendationStatus.Complete
            )
            .Returns([new NotifySendResult { Recipient = Recipient, Errors = ["Send failed"] }]);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareRecommendationAsync(
            controller,
            CategorySlug,
            SectionSlug,
            SecondChunkSlug,
            InputModel
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("NotifyError", redirect.ActionName);
        Assert.Equal("Pages", redirect.ControllerName);
        var shareResults = Assert.IsType<string>(
            controller.TempData[StatePassingMechanismConstants.NotifyShareResults]
        );
        Assert.Contains("Send failed", shareResults);
    }
}
