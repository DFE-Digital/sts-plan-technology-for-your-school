using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Helpers;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Sql;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders.PagesViewBuilderTests;

public class RouteToShareStandardPageAsyncTests : PagesViewBuilderTestBase
{
    private const string CategorySlug = "category-slug";
    private const string CategoryHeader = "Category header";
    private const string ShareByEmailView = "~/Views/Shared/Email/ShareByEmail.cshtml";
    private const string SectionId = "section-1";
    private const int FocusedEstablishmentId = 123;
    private const string FocusedSchoolName = "Springfield Primary";
    private const string MatName = "Springfield Academy Trust";
    private const string Recipient = "recipient@example.com";

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenCategoryIsNotFound_RedirectsToHomePage()
    {
        // Arrange
        _contentfulService
            .GetCategoryBySlugAsync("missing-category", 4)
            .Returns((QuestionnaireCategoryEntry?)null);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareStandardPageAsync(
            controller,
            "missing-category",
            BuildInputModel()
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.Equal(nameof(PagesController).GetControllerNameSlug(), redirect.ControllerName);
        Assert.Equal(UrlConstants.HomePage, redirect.RouteValues!["route"]);
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenInputModelIsNull_ReturnsShareViewForSchool()
    {
        // Arrange
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(BuildCategory());
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareStandardPageAsync(controller, CategorySlug);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ShareByEmailView, view.ViewName);
        var viewModel = Assert.IsType<ShareByEmailViewModel>(view.Model);
        Assert.Equal(nameof(PagesController).GetControllerNameSlug(), viewModel.PostController);
        Assert.Equal(nameof(PagesController.ShareStandard), viewModel.PostAction);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.Equal(CategoryHeader, viewModel.Caption);
        Assert.Equal("Share list of recommendations by email", viewModel.Heading);
        Assert.Null(viewModel.InputModel);
        Assert.False(viewModel.IsMat);
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenInputModelIsNullAndContextIsMat_ReturnsShareViewForMat()
    {
        // Arrange
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(BuildCategory());
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareStandardPageAsync(
            controller,
            CategorySlug,
            context: CategoryLandingContext.MAT
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ShareByEmailView, view.ViewName);
        var viewModel = Assert.IsType<ShareByEmailViewModel>(view.Model);
        Assert.Equal(nameof(PagesController.ShareMatStandard), viewModel.PostAction);
        Assert.Equal(CategorySlug, viewModel.CategorySlug);
        Assert.True(viewModel.IsMat);
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenModelStateIsInvalid_ReturnsShareViewWithInputModelAndDoesNotSendEmail()
    {
        // Arrange
        var inputModel = BuildInputModel();
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(BuildCategory());
        var controller = new TestController();
        controller.ModelState.AddModelError("EmailAddresses", "Enter a valid email address");
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareStandardPageAsync(
            controller,
            CategorySlug,
            inputModel
        );

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ShareByEmailView, view.ViewName);
        var viewModel = Assert.IsType<ShareByEmailViewModel>(view.Model);
        Assert.Same(inputModel, viewModel.InputModel);
        _notifyService
            .DidNotReceiveWithAnyArgs()
            .SendStandardEmail(default!, default!, default!, default!, default!, default!);
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenSchoolEmailSendsSuccessfully_SendsStandardEmailForFocusedSchool()
    {
        // Arrange
        var inputModel = BuildInputModel();
        var category = BuildCategory();
        var sectionStatuses = new List<SqlSectionStatusDto>
        {
            new() { SectionId = SectionId, Status = SubmissionStatus.CompleteReviewed },
        };
        var recommendationStatuses = new Dictionary<
            string,
            SqlEstablishmentRecommendationHistoryDto
        >
        {
            { "recommendation-1", new SqlEstablishmentRecommendationHistoryDto() },
        };
        ArrangeFocusedSchool();
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(category);
        _recommendationService
            .GetLatestRecommendationStatusesAsync(FocusedEstablishmentId)
            .Returns(recommendationStatuses);
        _submissionService
            .GetSectionStatusesForSchoolAsync(
                FocusedEstablishmentId,
                Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { SectionId }))
            )
            .Returns(sectionStatuses);
        _notifyService
            .SendStandardEmail(
                Arg.Any<ShareByEmailModel>(),
                category.Sections,
                sectionStatuses,
                recommendationStatuses,
                CategoryHeader,
                FocusedSchoolName
            )
            .Returns([new NotifySendResult { Recipient = Recipient, Errors = [] }]);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareStandardPageAsync(
            controller,
            CategorySlug,
            inputModel
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PagesController.GetByRoute), redirect.ActionName);
        Assert.Equal(nameof(PagesController).GetControllerNameSlug(), redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues!["route"]);
        _notifyService
            .Received(1)
            .SendStandardEmail(
                Arg.Is<ShareByEmailModel>(model =>
                    model.NameOfUser == inputModel.NameOfUser
                    && model.EmailAddresses.SequenceEqual(inputModel.EmailAddresses)
                    && model.UserMessage == inputModel.UserMessage
                ),
                category.Sections,
                sectionStatuses,
                recommendationStatuses,
                CategoryHeader,
                FocusedSchoolName
            );
        _notifyService
            .DidNotReceiveWithAnyArgs()
            .SendMatStandardEmail(default!, default!, default!, default!);
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenSchoolEmailHasErrors_RedirectsToNotifyError()
    {
        // Arrange
        ArrangeFocusedSchool();
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(BuildCategory());
        _notifyService
            .SendStandardEmail(
                Arg.Any<ShareByEmailModel>(),
                Arg.Any<List<QuestionnaireSectionEntry>>(),
                Arg.Any<List<SqlSectionStatusDto>>(),
                Arg.Any<Dictionary<string, SqlEstablishmentRecommendationHistoryDto>>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            )
            .Returns([new NotifySendResult { Recipient = Recipient, Errors = ["send failed"] }]);
        var controller = new TestController();
        var expectedRedirect = PageRedirecter.RedirectToNotifyError(controller);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareStandardPageAsync(
            controller,
            CategorySlug,
            BuildInputModel()
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(expectedRedirect.ActionName, redirect.ActionName);
        Assert.Equal(expectedRedirect.ControllerName, redirect.ControllerName);
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenMatEmailSendsSuccessfully_SendsMatStandardEmailAndRedirectsToMatLanding()
    {
        // Arrange
        var inputModel = BuildInputModel();
        var category = BuildCategory();
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(category);
        _currentUser
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(MatName).Build());
        _notifyService
            .SendMatStandardEmail(
                Arg.Any<ShareByEmailModel>(),
                category.Sections,
                MatName,
                CategoryHeader
            )
            .Returns([new NotifySendResult { Recipient = Recipient, Errors = [] }]);
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareStandardPageAsync(
            controller,
            CategorySlug,
            inputModel,
            CategoryLandingContext.MAT
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(GroupsController.GetMatRecommendationsLanding), redirect.ActionName);
        Assert.Equal(nameof(GroupsController).GetControllerNameSlug(), redirect.ControllerName);
        Assert.Equal(CategorySlug, redirect.RouteValues!["categorySlug"]);
        _notifyService
            .Received(1)
            .SendMatStandardEmail(
                Arg.Is<ShareByEmailModel>(model =>
                    model.NameOfUser == inputModel.NameOfUser
                    && model.EmailAddresses.SequenceEqual(inputModel.EmailAddresses)
                    && model.UserMessage == inputModel.UserMessage
                ),
                category.Sections,
                MatName,
                CategoryHeader
            );
        _notifyService
            .DidNotReceiveWithAnyArgs()
            .SendStandardEmail(default!, default!, default!, default!, default!, default!);
        await _currentUser.DidNotReceive().GetFocusedEstablishmentAsync();
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenMatEmailHasErrors_RedirectsToNotifyError()
    {
        // Arrange
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(BuildCategory());
        _currentUser
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(MatName).Build());
        _notifyService
            .SendMatStandardEmail(
                Arg.Any<ShareByEmailModel>(),
                Arg.Any<List<QuestionnaireSectionEntry>>(),
                MatName,
                CategoryHeader
            )
            .Returns([new NotifySendResult { Recipient = Recipient, Errors = ["send failed"] }]);
        var controller = new TestController();
        var expectedRedirect = PageRedirecter.RedirectToNotifyError(controller);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.RouteToShareStandardPageAsync(
            controller,
            CategorySlug,
            BuildInputModel(),
            CategoryLandingContext.MAT
        );

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(expectedRedirect.ActionName, redirect.ActionName);
        Assert.Equal(expectedRedirect.ControllerName, redirect.ControllerName);
    }

    [Fact]
    public async Task RouteToShareStandardPageAsync_WhenMatNameIsMissing_ThrowsInvalidDataException()
    {
        // Arrange
        _contentfulService.GetCategoryBySlugAsync(CategorySlug, 4).Returns(BuildCategory());
        _currentUser
            .GetUserEstablishmentAsync()
            .Returns(new SqlEstablishmentDtoBuilder().WithOrgName(null).Build());
        var controller = new TestController();
        var systemUnderTest = CreateSystemUnderTest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            systemUnderTest.RouteToShareStandardPageAsync(
                controller,
                CategorySlug,
                BuildInputModel(),
                CategoryLandingContext.MAT
            )
        );
        Assert.Equal("Cannot send an email without a MAT name", exception.Message);
    }

    private void ArrangeFocusedSchool() =>
        _currentUser
            .GetFocusedEstablishmentAsync()
            .Returns(
                new SelectedEstablishmentDataModelBuilder()
                    .WithEstablishmentId(FocusedEstablishmentId)
                    .WithName(FocusedSchoolName)
                    .Build()
            );

    private static QuestionnaireCategoryEntry BuildCategory() =>
        new()
        {
            Header = new ComponentHeaderEntry { Text = CategoryHeader },
            Sections = [new QuestionnaireSectionEntry { Sys = new SystemDetails(SectionId) }],
        };

    private static ShareByEmailInputViewModel BuildInputModel() =>
        new()
        {
            NameOfUser = "Alex Morgan",
            EmailAddresses = [Recipient],
            UserMessage = "Please review these recommendations",
        };
}
