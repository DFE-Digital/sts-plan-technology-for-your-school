using System.Text.Json;
using Dfe.PlanTech.Application.Providers.Interfaces;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Tests.Shared.Builders.Models;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class BaseViewBuilderTests
{
    private const string Recipient = "recipient@example.com";
    private const string CategoryHeader = "Category heading";
    private const string CategorySlug = "category-slug";
    private const string SectionSlug = "section-slug";

    private readonly FakeLogger<BaseViewBuilder> _logger = new();
    private readonly IContentfulService _contentfulService = Substitute.For<IContentfulService>();
    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();

    private TestableBaseViewBuilder CreateSystemUnderTest() =>
        new(_logger, _contentfulService, _currentUser);

    [Fact]
    public void Constructor_WhenContentfulServiceIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TestableBaseViewBuilder(_logger, null!, _currentUser)
        );
        Assert.Equal("contentfulService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenCurrentUserIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TestableBaseViewBuilder(_logger, _contentfulService, null!)
        );
        Assert.Equal("currentUser", exception.ParamName);
    }

    [Fact]
    public void BuildShareByEmailViewModel_WhenChunkSlugIsNull_BuildsCategoryShareModel()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = CategoryHeader },
        };
        var inputModel = new ShareByEmailInputViewModel();

        // Act
        var result = TestableBaseViewBuilder.CallBuildShareByEmailViewModel(
            controllerName: "PagesController",
            actionName: "PostShare",
            category: category,
            chunk: null,
            categorySlug: CategorySlug,
            sectionSlug: SectionSlug,
            chunkSlug: null,
            inputModel: inputModel
        );

        // Assert
        Assert.Equal("Pages", result.PostController);
        Assert.Equal("PostShare", result.PostAction);
        Assert.Equal(CategorySlug, result.CategorySlug);
        Assert.Equal(SectionSlug, result.SectionSlug);
        Assert.Null(result.ChunkSlug);
        Assert.Equal(CategoryHeader, result.Caption);
        Assert.Equal("Share list of recommendations by email", result.Heading);
        Assert.Same(inputModel, result.InputModel);
        Assert.False(result.IsMat);
    }

    [Fact]
    public void BuildShareByEmailViewModel_WhenChunkSlugIsProvided_BuildsChunkShareModel()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = CategoryHeader },
        };
        var chunk = new RecommendationChunkEntry { Header = "Chunk heading" };

        // Act
        var result = TestableBaseViewBuilder.CallBuildShareByEmailViewModel(
            controllerName: "PagesController",
            actionName: "PostShare",
            category: category,
            chunk: chunk,
            categorySlug: CategorySlug,
            sectionSlug: SectionSlug,
            chunkSlug: "chunk-slug",
            inputModel: null
        );

        // Assert
        Assert.Equal("Pages", result.PostController);
        Assert.Equal("PostShare", result.PostAction);
        Assert.Equal(CategorySlug, result.CategorySlug);
        Assert.Equal(SectionSlug, result.SectionSlug);
        Assert.Equal("chunk-slug", result.ChunkSlug);
        Assert.Equal("Chunk heading", result.Caption);
        Assert.Equal("Share a recommendation by email", result.Heading);
        Assert.Null(result.InputModel);
    }

    [Fact]
    public void BuildShareByEmailViewModel_WhenIsMatIsTrue_SetsIsMat()
    {
        // Arrange
        var category = new QuestionnaireCategoryEntry
        {
            Header = new ComponentHeaderEntry { Text = CategoryHeader },
        };

        // Act
        var result = TestableBaseViewBuilder.CallBuildShareByEmailViewModel(
            controllerName: "PagesController",
            actionName: "PostShare",
            category: category,
            chunk: null,
            categorySlug: CategorySlug,
            sectionSlug: null,
            chunkSlug: null,
            inputModel: null,
            isMat: true
        );

        // Assert
        Assert.True(result.IsMat);
    }

    [Fact]
    public async Task GetFocusedOrSelectedSchoolsAsync_WhenUserCanSelectSchools_ReturnsSelectedSchools()
    {
        // Arrange
        var selectedSchools = new List<SelectedEstablishmentDataModel>
        {
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(1)
                .WithName("Springfield Primary")
                .Build(),
            new SelectedEstablishmentDataModelBuilder()
                .WithEstablishmentId(2)
                .WithName("Shelbyville Primary")
                .Build(),
        };
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser.GetSelectedSchools().Returns(selectedSchools);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.CallGetFocusedOrSelectedSchoolsAsync();

        // Assert
        Assert.Same(selectedSchools, result);
    }

    [Fact]
    public async Task GetFocusedOrSelectedSchoolsAsync_WhenUserCanSelectSchoolsAndHasNoSelectedSchools_ReturnsEmptyList()
    {
        // Arrange
        _currentUser.CanSelectSchools.Returns(true);
        _currentUser.GetSelectedSchools().Returns([]);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.CallGetFocusedOrSelectedSchoolsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFocusedOrSelectedSchoolsAsync_WhenUserCannotSelectSchools_ReturnsFocusedSchoolOnly()
    {
        // Arrange
        var focusedSchool = new SelectedEstablishmentDataModelBuilder()
            .WithEstablishmentId(3)
            .WithName("Ogdenville Primary")
            .Build();
        _currentUser.CanSelectSchools.Returns(false);
        _currentUser.GetFocusedEstablishmentAsync().Returns(focusedSchool);
        var systemUnderTest = CreateSystemUnderTest();

        // Act
        var result = await systemUnderTest.CallGetFocusedOrSelectedSchoolsAsync();

        // Assert
        var school = Assert.Single(result);
        Assert.Same(focusedSchool, school);
        _currentUser.DidNotReceive().GetSelectedSchools();
    }

    [Fact]
    public void HandleNotifyShareResults_WhenAllResultsSucceed_StoresResultsInTempDataAndRedirectsToReturnAction()
    {
        // Arrange
        var controller = new TestController();
        var notifySendResults = new List<NotifySendResult>
        {
            new() { Recipient = Recipient, Errors = [] },
            new() { Recipient = "second@example.com", Errors = [] },
        };
        var returnToModel = new ActionViewModel(
            "GetByRoute",
            "PagesController",
            "Back to category",
            new Dictionary<string, object> { { "route", "digital-leadership-and-governance" } }
        );

        // Act
        var result = TestableBaseViewBuilder.CallHandleNotifyShareResults(
            controller,
            notifySendResults,
            returnToModel
        );

        // Assert
        Assert.Equal("GetByRoute", result.ActionName);
        Assert.Equal("Pages", result.ControllerName);
        Assert.Equal("digital-leadership-and-governance", result.RouteValues!["route"]);
        var json = Assert.IsType<string>(
            controller.TempData[StatePassingMechanismConstants.NotifyShareResults]
        );
        var storedResults = JsonSerializer.Deserialize<NotifyShareResultsViewModel>(json);
        Assert.NotNull(storedResults);
        Assert.Equal(2, storedResults.SendResults.Count);
        Assert.Equal("GetByRoute", storedResults.ActionModel.ActionName);
        Assert.Equal("Pages", storedResults.ActionModel.ControllerName);
    }

    [Fact]
    public void HandleNotifyShareResults_WhenAnyResultHasErrors_StoresResultsInTempDataAndRedirectsToNotifyError()
    {
        // Arrange
        var controller = new TestController();
        var notifySendResults = new List<NotifySendResult>
        {
            new() { Recipient = Recipient, Errors = [] },
            new() { Recipient = "failed@example.com", Errors = ["send failed"] },
        };
        var returnToModel = new ActionViewModel(
            "GetByRoute",
            "PagesController",
            "Back to category",
            new Dictionary<string, object> { { "route", "digital-leadership-and-governance" } }
        );
        var expectedRedirect = PageRedirecter.RedirectToNotifyError(controller);

        // Act
        var result = TestableBaseViewBuilder.CallHandleNotifyShareResults(
            controller,
            notifySendResults,
            returnToModel
        );

        // Assert
        Assert.Equal(expectedRedirect.ActionName, result.ActionName);
        Assert.Equal(expectedRedirect.ControllerName, result.ControllerName);
        Assert.Equal(expectedRedirect.RouteValues, result.RouteValues);
        var json = Assert.IsType<string>(
            controller.TempData[StatePassingMechanismConstants.NotifyShareResults]
        );
        var storedResults = JsonSerializer.Deserialize<NotifyShareResultsViewModel>(json);
        Assert.NotNull(storedResults);
        Assert.Equal(2, storedResults.SendResults.Count);
    }

    private sealed class TestableBaseViewBuilder(
        ILogger<BaseViewBuilder> logger,
        IContentfulService contentfulService,
        ICurrentUserProvider currentUser
    ) : BaseViewBuilder(logger, contentfulService, currentUser)
    {
        public Task<List<SelectedEstablishmentDataModel>> CallGetFocusedOrSelectedSchoolsAsync() =>
            GetFocusedOrSelectedSchoolsAsync();

        public static ShareByEmailViewModel CallBuildShareByEmailViewModel(
            string controllerName,
            string actionName,
            QuestionnaireCategoryEntry category,
            RecommendationChunkEntry? chunk,
            string categorySlug,
            string? sectionSlug,
            string? chunkSlug,
            ShareByEmailInputViewModel? inputModel,
            bool isMat = false
        ) =>
            BuildShareByEmailViewModel(
                controllerName,
                actionName,
                category,
                chunk,
                categorySlug,
                sectionSlug,
                chunkSlug,
                inputModel,
                isMat
            );

        public static RedirectToActionResult CallHandleNotifyShareResults(
            Controller controller,
            List<NotifySendResult> notifySendResults,
            ActionViewModel returnToModel
        ) => HandleNotifyShareResults(controller, notifySendResults, returnToModel);
    }
}
