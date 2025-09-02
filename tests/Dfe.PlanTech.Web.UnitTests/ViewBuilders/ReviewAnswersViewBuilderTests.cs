using Dfe.PlanTech.Application.Services.Interfaces;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.RoutingDataModels;
using Dfe.PlanTech.Web.Context.Interfaces;
using Dfe.PlanTech.Web.ViewBuilders;
using Dfe.PlanTech.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class ReviewAnswersViewBuilderTests
{
    private readonly ILogger<BaseViewBuilder> _logger = Substitute.For<ILogger<BaseViewBuilder>>();
    private readonly IContentfulService _contentful = Substitute.For<IContentfulService>();
    private readonly ISubmissionService _submissions = Substitute.For<ISubmissionService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private ReviewAnswersViewBuilder CreateSut() =>
        new ReviewAnswersViewBuilder(_logger, _contentful, _submissions, _currentUser);

    private static Controller MakeController()
    {
        var ctl = new DummyController();
        var http = new DefaultHttpContext();
        ctl.ControllerContext = new ControllerContext { HttpContext = http };
        ctl.TempData = new TempDataDictionary(http, Substitute.For<ITempDataProvider>());
        return ctl;
    }

    private static QuestionnaireSectionEntry MakeSection(string id, string slug, string name = "Section", params QuestionnaireQuestionEntry[] qs)
        => new QuestionnaireSectionEntry { Sys = new SystemDetails(id), Name = name, Questions = qs?.ToList() ?? new List<QuestionnaireQuestionEntry>() };

    private static QuestionnaireQuestionEntry MakeQuestion(string id, string slug, string text = "Q")
        => new QuestionnaireQuestionEntry { Sys = new SystemDetails(id), Slug = slug, Text = text };

    private static SubmissionRoutingDataModel MakeRouting(
        SubmissionStatus status,
        QuestionnaireSectionEntry section,
        QuestionnaireQuestionEntry? next = null,
        DateTime? completed = null,
        params string[] answerIds)
        => new SubmissionRoutingDataModel(
            maturity: "medium",
            nextQuestion: next,
            questionnaireSection: section,
            submission: new SubmissionResponsesModel(123, answerIds.Select(a => new QuestionWithAnswerModel { AnswerSysId = a }).ToList())
            {
                DateCompleted = completed,
            },
            status);

    // ---------------- RouteToCheckAnswers ----------------

    [Fact]
    public async Task RouteToCheckAnswers_NotStarted_Redirects_Home()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.EstablishmentId.Returns(1);
        var section = MakeSection("S1", "sec-1");
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        _submissions.GetSubmissionRoutingDataAsync(1, section, false)
                    .Returns(MakeRouting(SubmissionStatus.NotStarted, section));

        var result = await sut.RouteToCheckAnswers(ctl, "cat", "sec-1", isChangeAnswersFlow: null);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Theory]
    [InlineData(SubmissionStatus.InProgress)]
    [InlineData(SubmissionStatus.CompleteNotReviewed)]
    [InlineData(SubmissionStatus.CompleteReviewed)] // with isChangeAnswersFlow != false → CheckAnswers
    public async Task RouteToCheckAnswers_Shows_CheckAnswers_For_Allowed_Statuses(SubmissionStatus status)
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.EstablishmentId.Returns(1);
        var section = MakeSection("S1", "sec-1", "Section 1");
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        // ensure page content is loaded only for CheckAnswers page
        _contentful.GetPageBySlugAsync(UrlConstants.CheckAnswersSlug)
                   .Returns(new PageEntry { Content = new List<ContentfulEntry> { new PageEntry { Sys = new SystemDetails("X") } } });

        _submissions.GetSubmissionRoutingDataAsync(1, section, false)
                    .Returns(MakeRouting(status, section, next: MakeQuestion("Q2", "q-2")));

        var result = await sut.RouteToCheckAnswers(ctl, "cat", "sec-1", isChangeAnswersFlow: null, errorMessage: "err");
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ReviewAnswersViewBuilder.CheckAnswersViewName, view.ViewName);

        var vm = Assert.IsType<ReviewAnswersViewModel>(view.Model);
        Assert.Equal(UrlConstants.CheckAnswersSlug, vm.Slug);
        Assert.Equal("Section 1", vm.SectionName);
        Assert.Equal("cat", vm.CategorySlug);
        Assert.Equal("sec-1", vm.SectionSlug);
        Assert.Equal("err", vm.ErrorMessage);
        Assert.NotNull(vm.Content);
        Assert.Single(vm.Content); // came from GetPageBySlugAsync
    }

    [Fact]
    public async Task RouteToCheckAnswers_CompleteReviewed_With_IsChangeAnswersFlow_False_Shows_ChangeAnswers()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.EstablishmentId.Returns(1);
        var section = MakeSection("S1", "sec-1", "Section 1");
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        _submissions.GetSubmissionRoutingDataAsync(1, section, false)
                    .Returns(MakeRouting(SubmissionStatus.CompleteReviewed, section, next: MakeQuestion("Q2", "q-2")));

        var result = await sut.RouteToCheckAnswers(ctl, "cat", "sec-1", isChangeAnswersFlow: false, errorMessage: null);
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ReviewAnswersViewBuilder.ChangeAnswersViewName, view.ViewName);

        var vm = Assert.IsType<ReviewAnswersViewModel>(view.Model);
        Assert.Equal(UrlConstants.ChangeAnswersSlug, vm.Slug);
        Assert.Equal("Section 1", vm.SectionName);
        // For ChangeAnswers page, BuildViewModel should NOT fetch page content (content stays empty)
        Assert.Empty(vm.Content ?? []);
    }

    [Fact]
    public async Task RouteToCheckAnswers_Default_Falls_Back_To_Redirect_To_Next_Question()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.EstablishmentId.Returns(1);
        var section = MakeSection("S1", "sec-1");
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        // Force a default path by spoofing a status not matched above (cast from int)
        var weirdStatus = (SubmissionStatus)999;
        var routing = MakeRouting(SubmissionStatus.InProgress, section, next: MakeQuestion("Q1", "q-1"));
        typeof(SubmissionRoutingDataModel).GetProperty(nameof(SubmissionRoutingDataModel.Status))!
            .SetValue(routing, weirdStatus);

        _submissions.GetSubmissionRoutingDataAsync(1, section, false).Returns(routing);

        var result = await sut.RouteToCheckAnswers(ctl, "cat", "sec-1", isChangeAnswersFlow: null);
        Assert.IsType<RedirectToActionResult>(result);
    }

    // ---------------- RouteToChangeAnswers ----------------

    [Fact]
    public async Task RouteToChangeAnswers_NotStarted_Redirects_Home()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.EstablishmentId.Returns(2);
        var section = MakeSection("S2", "sec-2");
        _contentful.GetSectionBySlugAsync("sec-2").Returns(section);

        _submissions.GetSubmissionRoutingDataAsync(2, section, true)
                    .Returns(MakeRouting(SubmissionStatus.NotStarted, section));

        var result = await sut.RouteToChangeAnswers(ctl, "cat", "sec-2");
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task RouteToChangeAnswers_InProgress_Redirects_To_Next_Unanswered()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.EstablishmentId.Returns(2);
        var section = MakeSection("S2", "sec-2");
        _contentful.GetSectionBySlugAsync("sec-2").Returns(section);

        _submissions.GetSubmissionRoutingDataAsync(2, section, true)
                    .Returns(MakeRouting(SubmissionStatus.InProgress, section));

        var result = await sut.RouteToChangeAnswers(ctl, "cat", "sec-2");
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task RouteToChangeAnswers_CompleteNotReviewed_Shows_CheckAnswers()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.EstablishmentId.Returns(2);
        var section = MakeSection("S2", "sec-2", "Section 2");
        _contentful.GetSectionBySlugAsync("sec-2").Returns(section);

        // Fetch page content for CheckAnswers
        _contentful.GetPageBySlugAsync(UrlConstants.CheckAnswersSlug)
                   .Returns(new PageEntry { Content = new List<ContentfulEntry> { new PageEntry { Sys = new SystemDetails("C") } } });

        _submissions.GetSubmissionRoutingDataAsync(2, section, true)
                    .Returns(MakeRouting(SubmissionStatus.CompleteNotReviewed, section));

        var result = await sut.RouteToChangeAnswers(ctl, "cat", "sec-2", "err");
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ReviewAnswersViewBuilder.CheckAnswersViewName, view.ViewName);
        var vm = Assert.IsType<ReviewAnswersViewModel>(view.Model);
        Assert.Equal("err", vm.ErrorMessage);
        Assert.NotNull(vm.Content);
        Assert.Single(vm.Content);
        Assert.Equal(UrlConstants.CheckAnswersSlug, vm.Slug);
    }

    [Fact]
    public async Task RouteToChangeAnswers_CompleteReviewed_Clones_Submission_And_Shows_ChangeAnswers()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.EstablishmentId.Returns(77);
        var q1 = MakeQuestion("Q1", "q-1");
        var section = MakeSection("S7", "sec-7", "Section 7", q1);
        _contentful.GetSectionBySlugAsync("sec-7").Returns(section);

        // Initial routing: CompleteReviewed to trigger clone path
        _submissions.GetSubmissionRoutingDataAsync(77, section, true)
                    .Returns(MakeRouting(SubmissionStatus.CompleteReviewed, section));

        // When cloning most recent completed submission:
        var cloned = new SqlSubmissionDto
        {
            Id = 999,
            Status = SubmissionStatus.CompleteNotReviewed.ToString(),
            Responses = new List<SqlResponseDto>
            {
                new SqlResponseDto
                {
                    Question = new SqlQuestionDto
                    {
                        ContentfulSysId = "Q1"
                    },
                    Answer = new SqlAnswerDto
                    {
                        ContentfulSysId = "A1"
                    }
                }
            }
        };
        _submissions.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(77, section)
                    .Returns(cloned);

        var result = await sut.RouteToChangeAnswers(ctl, "cat", "sec-7");

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ReviewAnswersViewBuilder.ChangeAnswersViewName, view.ViewName);

        var vm = Assert.IsType<ReviewAnswersViewModel>(view.Model);
        Assert.Equal(UrlConstants.ChangeAnswersSlug, vm.Slug);
        Assert.Equal("Section 7", vm.SectionName);
        Assert.Equal(999, vm.SubmissionId);
        Assert.NotNull(vm.SubmissionResponses);
        Assert.NotNull(vm.SubmissionResponses.Responses);
        Assert.Contains(vm.SubmissionResponses.Responses, r => r.AnswerRef == "A1");

        // Ensure the clone was requested
        await _submissions.Received(1).RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(77, section);
    }

    [Fact]
    public async Task RouteToChangeAnswers_Default_Redirects_To_GetQuestionBySlug()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.EstablishmentId.Returns(2);
        var q1 = MakeQuestion("QX", "q-x");
        var section = MakeSection("S2", "sec-2", "S", q1);
        _contentful.GetSectionBySlugAsync("sec-2").Returns(section);

        var routing = MakeRouting((SubmissionStatus)999, section, next: q1);
        _submissions.GetSubmissionRoutingDataAsync(2, section, true).Returns(routing);

        var result = await sut.RouteToChangeAnswers(ctl, "cat", "sec-2");
        Assert.IsType<RedirectToActionResult>(result);
    }

    // ---------------- ConfirmCheckAnswers ----------------

    [Fact]
    public async Task ConfirmCheckAnswers_Success_Redirects_To_Category_Landing_And_Sets_SectionName()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _submissions.ConfirmCheckAnswersAsync(42).Returns(Task.CompletedTask);

        var result = await sut.ConfirmCheckAnswers(ctl, "cat", "sec", "My Section", 42);

        await _submissions.Received(1).ConfirmCheckAnswersAsync(42);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.True(ctl.TempData.ContainsKey("SectionName"));
        Assert.Equal("My Section", ctl.TempData["SectionName"]);
        Assert.NotNull(redirect); // usually goes to category landing page helper → RedirectToActionResult
    }

    [Fact]
    public async Task ConfirmCheckAnswers_Failure_Sets_Error_TempData_And_Redirects_Back_To_CheckAnswers()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _submissions.ConfirmCheckAnswersAsync(9).Returns<Task>(_ => throw new Exception("boom"));

        var result = await sut.ConfirmCheckAnswers(ctl, "cat", "sec", "S", 9);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.True(ctl.TempData.ContainsKey("ErrorMessage"));
        Assert.Equal(ReviewAnswersViewBuilder.InlineRecommendationUnavailableErrorMessage, ctl.TempData["ErrorMessage"]);
        // Route values should carry category & section back to Check Answers page
        Assert.Equal("cat", redirect.RouteValues["categorySlug"]);
        Assert.Equal("sec", redirect.RouteValues["sectionSlug"]);
    }

    private sealed class DummyController : Controller { }
}
