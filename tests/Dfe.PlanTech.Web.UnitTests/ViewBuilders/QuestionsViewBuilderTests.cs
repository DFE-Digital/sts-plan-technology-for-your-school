using Dfe.PlanTech.Application.Configuration;
using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class QuestionsViewBuilderTests
{
    // Collaborators
    private readonly ILogger<BaseViewBuilder> _logger = Substitute.For<ILogger<BaseViewBuilder>>();
    private readonly IContentfulService _contentful = Substitute.For<IContentfulService>();
    private readonly IQuestionService _questionSvc = Substitute.For<IQuestionService>();
    private readonly ISubmissionService _submissionSvc = Substitute.For<ISubmissionService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    // Options
    private readonly IOptions<ContactOptionsConfiguration> _contactOptions =
        Options.Create(new ContactOptionsConfiguration { LinkId = "contact-link-id" });

    private readonly IOptions<ErrorMessagesConfiguration> _errorMessages =
        Options.Create(new ErrorMessagesConfiguration
        {
            ConcurrentUsersOrContentChange = "There was a problem, please contact us."
        });

    private ContentfulOptionsConfiguration _contentfulOptions = new ContentfulOptionsConfiguration
    {
        UsePreviewApi = false
    };

    private QuestionsViewBuilder CreateServiceUnderTest()
        => new QuestionsViewBuilder(
            _logger,
            _contactOptions,
            _errorMessages,
            _contentful,
            _questionSvc,
            _submissionSvc,
            _contentfulOptions,
            _currentUser);

    private static Controller MakeControllerWithTempData()
    {
        var controller = new DummyController();
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var tempDataProvider = Substitute.For<ITempDataProvider>();
        controller.TempData = new TempDataDictionary(httpContext, tempDataProvider);
        return controller;
    }

    private static QuestionnaireSectionEntry MakeSection(string id, string slug, string name, params QuestionnaireQuestionEntry[] questions)
        => new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails(id),
            Name = name,
            Questions = questions,
            InterstitialPage = new PageEntry { Slug = slug }
        };

    private static QuestionnaireQuestionEntry MakeQuestion(string id, string slug, string text)
        => new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails(id),
            Slug = slug,
            Text = text,
            Answers = new List<QuestionnaireAnswerEntry>()
        };

    private static SubmissionRoutingDataModel MakeSubmissionRoutingDataModel(
        QuestionnaireSectionEntry section,
        SubmissionStatus submissionStatus
    )
    {
        var nextQuestion = MakeQuestion("Q2", "question2", "Question 2");
        var submissionResponses = new SubmissionResponsesModel(1, []);

        return new SubmissionRoutingDataModel(
            nextQuestion,
            section,
            submissionResponses,
            submissionStatus
        );
    }

    // ---------- RouteByQuestionId ----------

    [Fact]
    public async Task RouteByQuestionId_When_Preview_Disabled_Redirects_To_Home()
    {
        // Arrange
        _contentfulOptions = new ContentfulOptionsConfiguration { UsePreviewApi = false };
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        // Act
        var result = await sut.RouteByQuestionId(controller, "any-id");

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(UrlConstants.HomePage, redirect.Url);
    }

    [Fact]
    public async Task RouteByQuestionId_When_Preview_Enabled_Returns_Question_View()
    {
        // Arrange
        _contentfulOptions = new ContentfulOptionsConfiguration { UsePreviewApi = true };
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        var question = MakeQuestion("Q1", "q-1", "Question text");
        _contentful.GetQuestionByIdAsync("Q1").Returns(question);

        // Act
        var result = await sut.RouteByQuestionId(controller, "Q1");

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", view.ViewName);
        var vm = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.Equal("Question text", controller.ViewData["Title"]);
        Assert.Equal(question, vm.Question);
    }

    // ---------- RouteToInterstitialPage ----------

    [Fact]
    public async Task RouteToInterstitialPage_Returns_Interstitial_View_With_PageViewModel()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();
        var page = new PageEntry { Slug = "section-slug", SectionTitle = "Interstitial" };
        _contentful.GetPageBySlugAsync("section-slug").Returns(page);

        // Act
        var result = await sut.RouteToInterstitialPage(controller, "cat", "section-slug");

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Pages/Page.cshtml", view.ViewName);
        Assert.IsType<PageViewModel>(view.Model);
    }

    // ---------- RouteToNextUnansweredQuestion ----------

    [Fact]
    public async Task RouteToNextUnansweredQuestion_When_NextQuestion_Exists_Redirects_To_GetQuestionBySlug()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        // Current user info needed by BaseViewBuilder
        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);

        var section = MakeSection("S1", "sec-1", "Section", MakeQuestion("Q1", "q-1", "Text"));
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        var nextQ = MakeQuestion("Q2", "q-2", "Next text");
        _questionSvc.GetNextUnansweredQuestion(123, section).Returns(nextQ);

        // Act
        var result = await sut.RouteToNextUnansweredQuestion(controller, "cat", "sec-1");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetQuestionBySlug), redirect.ActionName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal("cat", redirect.RouteValues["categorySlug"]);
        Assert.Equal("sec-1", redirect.RouteValues["sectionSlug"]);
        Assert.Equal("q-2", redirect.RouteValues["questionSlug"]);
    }

    [Fact]
    public async Task RouteToNextUnansweredQuestion_When_No_NextQuestion_Redirects_To_CheckAnswers()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        var section = MakeSection("S1", "sec-1", "Section");
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        _questionSvc.GetNextUnansweredQuestion(123, section).Returns((QuestionnaireQuestionEntry?)null);

        // Act
        var result = await sut.RouteToNextUnansweredQuestion(controller, "cat", "sec-1");

        // Assert
        // Helper returns a RedirectToActionResult – only assert that we got a redirect with expected route values
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal("cat", redirect.RouteValues["categorySlug"]);
        Assert.Equal("sec-1", redirect.RouteValues["sectionSlug"]);
    }

    [Fact]
    public async Task RouteToNextUnansweredQuestion_On_DatabaseException_Deletes_Submission_Sets_TempData_And_Redirects_Home()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(987);
        var section = MakeSection("S99", "sec-err", "Section Err");
        _contentful.GetSectionBySlugAsync("sec-err").Returns(section);

        // Make NextUnanswered throw DatabaseException -> triggers cleanup/redirect path
        _questionSvc.GetNextUnansweredQuestion(987, section).Returns<Task<QuestionnaireQuestionEntry?>>(_ => throw new DatabaseException("boom"));

        // BuildErrorMessage needs link
        _contentful.GetLinkByIdAsync("contact-link-id")
                   .Returns(new NavigationLinkEntry { Href = "https://example.org/contact" });

        // Act
        var result = await sut.RouteToNextUnansweredQuestion(controller, "cat", "sec-err");

        // Assert
        await _submissionSvc.Received(1).SetSubmissionInaccessibleAsync(987, "S99");

        Assert.True(controller.TempData.ContainsKey("SubtopicError"));
        var msg = controller.TempData["SubtopicError"] as string;
        Assert.Contains("<a href=\"https://example.org/contact\"", msg);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(PagesController.GetPageByRouteAction, redirect.ActionName);
        Assert.Equal(PagesController.ControllerName, redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(UrlConstants.Home, redirect.RouteValues["route"]);
    }

    // ---------- SubmitAnswerAndRedirect ----------

    [Fact]
    public async Task SubmitAnswerAndRedirect_When_ModelState_Invalid_Returns_Question_View_With_Errors()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();
        controller.ModelState.AddModelError("Answer", "Answer is required");

        _currentUser.UserId.Returns(11);
        _currentUser.UserOrganisationId.Returns(22);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(22);

        var q = MakeQuestion("Q1", "q-1", "Question 1");
        var section = MakeSection("S1", "sec-1", "Section 1", q);
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        // submission routing is fetched but not used on the invalid-path (still harmless to return a simple stub)
        _submissionSvc.GetSubmissionRoutingDataAsync(22, section, false)
                      .Returns(MakeSubmissionRoutingDataModel(section, SubmissionStatus.InProgress));

        var vm = new SubmitAnswerInputViewModel
        {
            // No chosen answer -> invalid per ModelState error above
        };

        // Act
        var result = await sut.SubmitAnswerAndRedirect(controller, vm, "cat", "sec-1", "q-1", returnTo: null);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", view.ViewName);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.NotNull(model.ErrorMessages);
        Assert.Contains("Answer is required", model.ErrorMessages);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_When_Submit_Fails_Shows_Error_In_Question_View()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        _currentUser.UserId.Returns(11);
        _currentUser.UserOrganisationId.Returns(22);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(22);

        var q = MakeQuestion("Q1", "q-1", "Question 1");
        var section = MakeSection("S1", "sec-1", "Section 1", q);
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        _submissionSvc.GetSubmissionRoutingDataAsync(22, section, false)
                      .Returns(MakeSubmissionRoutingDataModel(section, SubmissionStatus.InProgress));

        var vm = new SubmitAnswerInputViewModel
        {
            ChosenAnswerJson = @"{ ""answer"": { ""id"": ""A1"" } }"
        };

        // User logged in directly as a school, therefore activeEstablishmentId == userEstablishmentId
        _submissionSvc.SubmitAnswerAsync(11, 22, 22, Arg.Any<SubmitAnswerModel>())
                      .Returns<Task>(_ => throw new Exception("db down"));

        // Act
        var result = await sut.SubmitAnswerAndRedirect(controller, vm, "cat", "sec-1", "q-1", returnTo: null);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Question", view.ViewName);
        var model = Assert.IsType<QuestionViewModel>(view.Model);
        Assert.NotNull(model.ErrorMessages);
        Assert.Contains("Save failed. Please try again later.", model.ErrorMessages);
    }

    [Fact]
    public async Task SubmitAnswerAndRedirect_Happy_Path_Redirects_To_Next_Unanswered()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        _currentUser.UserId.Returns(11);
        _currentUser.UserOrganisationId.Returns(22);
        _currentUser.GetActiveEstablishmentIdAsync().Returns(22);

        var q1 = MakeQuestion("Q1", "q-1", "Q1");
        var q2 = MakeQuestion("Q2", "question2", "Q2");
        var section = MakeSection("S1", "sec-1", "Section 1", q1, q2);
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        _submissionSvc.GetSubmissionRoutingDataAsync(22, section, false)
                      .Returns(MakeSubmissionRoutingDataModel(section, SubmissionStatus.InProgress));

        var vm = new SubmitAnswerInputViewModel
        {
            ChosenAnswerJson = @"{""answer"": { ""id"": ""A1"" } }"
        };

        // Submit works
        // User logged in directly as a school, therefore activeEstablishmentId == userEstablishmentId
        _submissionSvc.SubmitAnswerAsync(11, 22, 22, Arg.Any<SubmitAnswerModel>()).Returns(1);

        _questionSvc.GetNextUnansweredQuestion(22, section).Returns(q2);

        // Act
        var result = await sut.SubmitAnswerAndRedirect(controller, vm, "cat", "sec-1", "q-1", returnTo: null);

        // Assert
        await _submissionSvc.Received(1).SubmitAnswerAsync(11, 22, 22, Arg.Any<SubmitAnswerModel>());

        var redirect = Assert.IsType<ViewResult>(result);
    }

    // ---------- RouteToContinueSelfAssessmentPage ----------

    [Fact]
    public async Task RouteToContinueSelfAssessmentPage_When_No_Responses_Redirects_To_Interstitial()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        _currentUser.GetActiveEstablishmentNameAsync().Returns("Everwood Learning Trust");

        var sectionSlug = "sec-1";
        var section = MakeSection("S1", sectionSlug, "Section 1");
        _contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        var empty = new SubmissionResponsesModel(1, new List<QuestionWithAnswerModel>());
        _submissionSvc.GetLatestSubmissionResponsesModel(123, section, false).Returns(empty);

        // Act
        var result = await sut.RouteToContinueSelfAssessmentPage(controller, "cat", sectionSlug);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(PagesController.GetPageByRouteAction, redirect.ActionName);
        Assert.Equal(PagesController.ControllerName, redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal(sectionSlug, redirect.RouteValues["route"]);
    }

    [Fact]
    public async Task RouteToContinueSelfAssessmentPage_When_Responses_Exist_Returns_View_With_ViewModel()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(123);
        _currentUser.GetActiveEstablishmentNameAsync().Returns("Test Trust");

        var q1 = MakeQuestion("Q1", "q-1", "Question 1");
        var q2 = MakeQuestion("Q2", "q-2", "Question 2");

        var section = MakeSection("S1", "sec-1", "Cyber security processes", q1, q2);
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        var submissionWithResponses = new SubmissionResponsesModel(
            1,
            new List<QuestionWithAnswerModel> { new QuestionWithAnswerModel { AnswerSysId = "A1" } }
        )
        {
            Establishment = new SqlEstablishmentDto { OrgName = "Test Trust" }
        };

        _submissionSvc.GetLatestSubmissionResponsesModel(123, section, false)
                      .Returns(submissionWithResponses);

        // Act
        var result = await sut.RouteToContinueSelfAssessmentPage(controller, "category-slug", "sec-1");

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("ContinueSelfAssessment", view.ViewName);

        var vm = Assert.IsType<ContinueSelfAssessmentViewModel>(view.Model);
        Assert.Equal(1, vm.AnsweredCount);
        Assert.Equal(2, vm.QuestionsCount);
        Assert.Equal("category-slug", vm.CategorySlug);
        Assert.Equal("sec-1", vm.SectionSlug);
        Assert.Equal("Cyber security processes", vm.TopicName);
        Assert.Same(submissionWithResponses.Responses, vm.Responses);
    }

    // ---------- RestartSelfAssessment ----------

    [Fact]
    public async Task RestartSelfAssessment_Deletes_Submission_And_Redirects_To_Interstitial()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(555);

        var sectionSlug = "sec-restart";
        var section = MakeSection("S123", sectionSlug, "Restart Section");
        _contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        // Act
        var result = await sut.RestartSelfAssessment(controller, "cat-slug", sectionSlug, false);

        // Assert
        await _submissionSvc.Received(1).SetSubmissionInaccessibleAsync(555, "S123");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetInterstitialPage), redirect.ActionName);
        Assert.Equal(QuestionsController.Controller, redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal("cat-slug", redirect.RouteValues["categorySlug"]);
        Assert.Equal(sectionSlug, redirect.RouteValues["sectionSlug"]);
    }

    // ---------- ContinuePreviousAssessment ----------
    [Fact]
    public async Task ContinuePreviousAssessment_Restores_Submission_And_Redirects_To_Next_Unanswered()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var controller = MakeControllerWithTempData();

        _currentUser.GetActiveEstablishmentIdAsync().Returns(555);

        var sectionSlug = "sec-continue-prev";
        var section = MakeSection("S123", sectionSlug, "Continue previous assessment");
        _contentful.GetSectionBySlugAsync(sectionSlug).Returns(section);

        // Act
        var result = await sut.ContinuePreviousAssessment(controller, "cat-slug", sectionSlug);

        // Assert
        await _submissionSvc.Received(1).RestoreInaccessibleSubmissionAsync(555, "S123");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.GetNextUnansweredQuestion), redirect.ActionName);
        Assert.Equal(QuestionsController.Controller, redirect.ControllerName);
        Assert.NotNull(redirect.RouteValues);
        Assert.Equal("cat-slug", redirect.RouteValues["categorySlug"]);
        Assert.Equal(sectionSlug, redirect.RouteValues["sectionSlug"]);
    }

    // ------------- Stubs / helpers -------------

    private sealed class DummyController : Controller { }
}
