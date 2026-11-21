using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests.Builders;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.RecommendationsViewBuilderTests;

public class RouteToShareRecommendationAsyncMatTests : RecommendationsViewBuilderTestBase
{
    private const string TextBodyId = "text-body-2";
    private const string Recipient = "recipient@example.com";
    private const string MatName = "Springfield Trust";

    private static readonly ComponentTextBodyEntry TextBody = new()
    {
        Sys = new SystemDetails(TextBodyId),
    };

    private static readonly SelectedEstablishmentDataModel AlphaSchool =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(1)
            .WithName("Alpha School")
            .Build();

    private static readonly SelectedEstablishmentDataModel BetaSchool =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(2)
            .WithName("beta School")
            .Build();

    private static readonly SelectedEstablishmentDataModel GammaSchool =
        new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(3)
            .WithName("Gamma School")
            .Build();

    private static ShareByEmailInputViewModel InputModel =>
        new()
        {
            NameOfUser = "Alex Morgan",
            EmailAddresses = [Recipient],
            UserMessage = "Worth a read",
        };

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenInputModelIsNull_ReturnsShareViewForMatSingleRecommendation()
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
            SecondChunkSlug,
            context: CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Shared/Email/ShareByEmail.cshtml", view.ViewName);
        var viewModel = Assert.IsType<ShareByEmailViewModel>(view.Model);
        Assert.Equal("Recommendations", viewModel.PostController);
        Assert.Equal("ShareMatSingleRecommendation", viewModel.PostAction);
        Assert.Equal("Second chunk", viewModel.Caption);
        Assert.True(viewModel.IsMat);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenMatNameIsMissing_ThrowsInvalidDataException()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().Build(),
            new QuestionnaireSectionEntryBuilder().Build()
        );
        _contentfulService.GetTextBodyByIdAsync(TextBodyId).Returns(TextBody);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(null).Build());
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.RouteToShareRecommendationAsync(
                new TestController(),
                CategorySlug,
                SectionSlug,
                SecondChunkSlug,
                InputModel,
                CategoryLandingContext.MAT
            )
        );
        Assert.Equal("Cannot send an email without a MAT name", exception.Message);
    }

    [Fact]
    public async Task RouteToShareRecommendationAsync_WhenEmailIsSent_SendsMatEmailWithCompletedSchoolStatusesAndRedirectsBack()
    {
        // Arrange
        ArrangeContentful(
            new QuestionnaireCategoryEntryBuilder().WithHeaderText("Networking").Build(),
            new QuestionnaireSectionEntryBuilder().WithName("Network security").Build()
        );
        _contentfulService.GetTextBodyByIdAsync(TextBodyId).Returns(TextBody);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(MatName).Build());
        _currentUserProvider.GetGroupSchoolsAsync().Returns([GammaSchool, BetaSchool, AlphaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 3, 2, 1 }))
            )
            .Returns([
                new SqlSubmissionDto { EstablishmentId = 1, SectionId = SectionId },
                new SqlSubmissionDto { EstablishmentId = 2, SectionId = SectionId },
                new SqlSubmissionDto { EstablishmentId = 3, SectionId = "other-section-id" },
            ]);
        _recommendationService
            .GetLatestRecommendationHistoryAsync(1, "chunk-2")
            .Returns(
                new SqlEstablishmentRecommendationHistoryDtoBuilder()
                    .WithNewStatus(RecommendationStatus.Complete)
                    .Build()
            );
        _recommendationService
            .GetLatestRecommendationHistoryAsync(2, "chunk-2")
            .Returns((SqlEstablishmentRecommendationHistoryDto?)null);
        var expectedSchoolsList =
            $"* Alpha School: Complete{Environment.NewLine}* beta School: Not\u00A0started";
        _notifyService
            .SendMatSingleRecommendationEmail(
                Arg.Is<ShareByEmailModel>(model =>
                    model.NameOfUser == "Alex Morgan"
                    && model.EmailAddresses.SequenceEqual(new[] { Recipient })
                ),
                TextBody,
                MatName,
                "Second chunk",
                "Networking",
                "Network security",
                expectedSchoolsList
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
            InputModel,
            CategoryLandingContext.MAT
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetMatSingleRecommendation", redirect.ActionName);
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
            new QuestionnaireSectionEntryBuilder().WithName("Network security").Build()
        );
        _contentfulService.GetTextBodyByIdAsync(TextBodyId).Returns(TextBody);
        _currentUserProvider
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(MatName).Build());
        _currentUserProvider.GetGroupSchoolsAsync().Returns([AlphaSchool]);
        _groupService
            .GetGroupCompletedSubmissionsBySections(
                Arg.Is<int[]>(ids => ids.SequenceEqual(new[] { 1 }))
            )
            .Returns([new SqlSubmissionDto { EstablishmentId = 1, SectionId = SectionId }]);
        _recommendationService
            .GetLatestRecommendationHistoryAsync(1, "chunk-2")
            .Returns(
                new SqlEstablishmentRecommendationHistoryDtoBuilder()
                    .WithNewStatus(RecommendationStatus.InProgress)
                    .Build()
            );
        _notifyService
            .SendMatSingleRecommendationEmail(
                Arg.Is<ShareByEmailModel>(model => model.NameOfUser == "Alex Morgan"),
                TextBody,
                MatName,
                "Second chunk",
                "Networking",
                "Network security",
                "* Alpha School: In\u00A0progress"
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
            InputModel,
            CategoryLandingContext.MAT
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("NotifyError", redirect.ActionName);
        Assert.Equal("Pages", redirect.ControllerName);
    }
}
