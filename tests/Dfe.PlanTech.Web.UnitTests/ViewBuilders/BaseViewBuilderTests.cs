using System.Security.Authentication;
using System.Text.Json;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class BaseViewBuilderTests
{
    private readonly ILogger<BaseViewBuilder> _logger = Substitute.For<ILogger<BaseViewBuilder>>();
    private readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private const string DefaultRecipient = "test@test.com";

    private TestableBaseViewBuilder CreateSut() => new(_logger, _contentfulService, _currentUser);

    private static TestController CreateController() => new TestController();

    [Fact]
    public void Constructor_WhenContentfulServiceIsNull_ThrowsArgumentNullException()
    {
        var act = () => new TestableBaseViewBuilder(_logger, null!, _currentUser);

        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("contentfulService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenCurrentUserIsNull_ThrowsArgumentNullException()
    {
        var act = () => new TestableBaseViewBuilder(_logger, _contentfulService, null!);

        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("currentUser", exception.ParamName);
    }

    [Fact]
    public void GetDsiReferenceOrThrowException_WhenDsiReferenceExists_ReturnsDsiReference()
    {
        _currentUser.DsiReference.Returns("dsi-123");

        var sut = CreateSut();

        var result = sut.CallGetDsiReferenceOrThrowException();

        Assert.Equal("dsi-123", result);
    }

    [Fact]
    public void GetDsiReferenceOrThrowException_WhenDsiReferenceIsNull_ThrowsAuthenticationException()
    {
        _currentUser.DsiReference.Returns((string?)null);

        var sut = CreateSut();

        var act = () => sut.CallGetDsiReferenceOrThrowException();

        var exception = Assert.Throws<AuthenticationException>(act);
        Assert.Equal("User is not authenticated", exception.Message);
    }

    [Fact]
    public void GetUserIdOrThrowException_WhenUserIdExists_ReturnsUserId()
    {
        _currentUser.UserId.Returns(123);

        var sut = CreateSut();

        var result = sut.CallGetUserIdOrThrowException();

        Assert.Equal(123, result);
    }

    [Fact]
    public void GetUserIdOrThrowException_WhenUserIdIsNull_ThrowsAuthenticationException()
    {
        _currentUser.UserId.Returns((int?)null);

        var sut = CreateSut();

        var act = () =>
        {
            sut.CallGetUserIdOrThrowException();
        };

        var exception = Assert.Throws<AuthenticationException>(act);
        Assert.Equal("User is not authenticated", exception.Message);
    }

    [Fact]
    public void GetUserOrganisationIdOrThrowException_WhenUserOrganisationIdExists_ReturnsUserOrganisationId()
    {
        _currentUser.UserOrganisationId.Returns(456);

        var sut = CreateSut();

        var result = sut.CallGetUserOrganisationIdOrThrowException();

        Assert.Equal(456, result);
    }

    [Fact]
    public void GetUserOrganisationIdOrThrowException_WhenUserOrganisationIdIsNull_ThrowsInvalidDataException()
    {
        _currentUser.UserOrganisationId.Returns((int?)null);

        var sut = CreateSut();

        var act = () =>
        {
            sut.CallGetUserOrganisationIdOrThrowException();
        };

        var exception = Assert.Throws<InvalidDataException>(act);
        Assert.Equal(nameof(ICurrentUser.UserOrganisationId), exception.Message);
    }

    [Fact]
    public async Task GetActiveEstablishmentIdOrThrowException_WhenActiveEstablishmentIdExists_ReturnsId()
    {
        _currentUser.GetActiveEstablishmentIdAsync().Returns(Task.FromResult<int?>(789));

        var sut = CreateSut();

        var result = await sut.CallGetActiveEstablishmentIdOrThrowException();

        Assert.Equal(789, result);
    }

    [Fact]
    public async Task GetActiveEstablishmentIdOrThrowException_WhenActiveEstablishmentIdIsNull_ThrowsInvalidDataException()
    {
        _currentUser.GetActiveEstablishmentIdAsync().Returns(Task.FromResult<int?>(null));

        var sut = CreateSut();

        var act = async () => await sut.CallGetActiveEstablishmentIdOrThrowException();

        var exception = await Assert.ThrowsAsync<InvalidDataException>(act);
        Assert.Equal(nameof(ICurrentUser.GetActiveEstablishmentIdAsync), exception.Message);
    }

    [Fact]
    public void BuildShareByEmailViewModel_WhenChunkSlugIsNull_BuildsCategoryShareModel()
    {
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = "Category heading" },
        };

        var inputModel = new ShareByEmailInputViewModel();

        var result = TestableBaseViewBuilder.CallBuildShareByEmailViewModel(
            controllerName: "PagesController",
            actionName: "PostShare",
            category: category,
            chunk: null,
            categorySlug: "category-slug",
            sectionSlug: "section-slug",
            chunkSlug: null,
            inputModel: inputModel
        );

        Assert.Equal("Pages", result.PostController);
        Assert.Equal("PostShare", result.PostAction);
        Assert.Equal("category-slug", result.CategorySlug);
        Assert.Equal("section-slug", result.SectionSlug);
        Assert.Null(result.ChunkSlug);
        Assert.Equal("Category heading", result.Caption);
        Assert.Equal("Share list of recommendations by email", result.Heading);
        Assert.Same(inputModel, result.InputModel);
    }

    [Fact]
    public void BuildShareByEmailViewModel_WhenChunkSlugIsProvided_BuildsChunkShareModel()
    {
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = "Category heading" },
        };

        var chunk = new RecommendationChunkEntry { Header = "Chunk heading" };

        var result = TestableBaseViewBuilder.CallBuildShareByEmailViewModel(
            controllerName: "PagesController",
            actionName: "PostShare",
            category: category,
            chunk: chunk,
            categorySlug: "category-slug",
            sectionSlug: "section-slug",
            chunkSlug: "chunk-slug",
            inputModel: null
        );

        Assert.Equal("Pages", result.PostController);
        Assert.Equal("PostShare", result.PostAction);
        Assert.Equal("category-slug", result.CategorySlug);
        Assert.Equal("section-slug", result.SectionSlug);
        Assert.Equal("chunk-slug", result.ChunkSlug);
        Assert.Equal("Chunk heading", result.Caption);
        Assert.Equal("Share a recommendation by email", result.Heading);
        Assert.Null(result.InputModel);
    }

    [Fact]
    public void HandleNotifyShareResults_WhenAllResultsSucceed_StoresResultsInTempDataAndRedirectsToReturnAction()
    {
        var controller = CreateController();

        var notifySendResults = new List<NotifySendResult>
        {
            new() { Recipient = DefaultRecipient, Errors = [] },
            new() { Recipient = DefaultRecipient, Errors = [] },
        };

        var returnToModel = new ActionViewModel(
            "GetByRoute",
            "PagesController",
            "LinkText",
            new { route = "digital-leadership-and-governance" }
        );

        var result = TestableBaseViewBuilder.CallHandleNotifyShareResults(
            controller,
            notifySendResults,
            returnToModel
        );

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("GetByRoute", redirect.ActionName);
        Assert.Equal("Pages", redirect.ControllerName);

        Assert.True(
            controller.TempData.ContainsKey(StatePassingMechanismConstants.NotifyShareResults)
        );

        var json = Assert.IsType<string>(
            controller.TempData[StatePassingMechanismConstants.NotifyShareResults]
        );
        var deserialized = JsonSerializer.Deserialize<NotifyShareResultsViewModel>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.SendResults.Count);
        Assert.Equal("GetByRoute", deserialized.ActionModel.ActionName);
        Assert.Equal("Pages", deserialized.ActionModel.ControllerName);
    }

    [Fact]
    public void HandleNotifyShareResults_WhenAnyResultHasErrors_StoresResultsInTempDataAndRedirectsToNotifyError()
    {
        var controller = CreateController();

        var notifySendResults = new List<NotifySendResult>
        {
            new() { Recipient = DefaultRecipient, Errors = ["boom"] },
        };

        var returnToModel = new ActionViewModel(
            "GetByRoute",
            "Pages",
            "LinkText",
            new { route = "digital-leadership-and-governance" }
        );

        var expected = controller.RedirectToNotifyError();

        var result = TestableBaseViewBuilder.CallHandleNotifyShareResults(
            controller,
            notifySendResults,
            returnToModel
        );

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(expected.ActionName, redirect.ActionName);
        Assert.Equal(expected.ControllerName, redirect.ControllerName);
        Assert.Equal(expected.RouteValues, redirect.RouteValues);

        Assert.True(
            controller.TempData.ContainsKey(StatePassingMechanismConstants.NotifyShareResults)
        );
    }
}
