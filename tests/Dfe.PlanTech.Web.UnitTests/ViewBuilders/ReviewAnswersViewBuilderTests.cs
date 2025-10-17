﻿using Dfe.PlanTech.Application.Services.Interfaces;
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
using NSubstitute.ExceptionExtensions;

namespace Dfe.PlanTech.Web.UnitTests.ViewBuilders;

public class ReviewAnswersViewBuilderTests
{
    private readonly ILogger<BaseViewBuilder> _logger = Substitute.For<ILogger<BaseViewBuilder>>();
    private readonly IContentfulService _contentful = Substitute.For<IContentfulService>();
    private readonly ISubmissionService _submissions = Substitute.For<ISubmissionService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private ReviewAnswersViewBuilder CreateSut()
    {
        _currentUser.ActiveEstablishmentId.Returns(2);
        _currentUser.UserOrganisationId.Returns((int?)null); // non-MAT user by default
        _currentUser.UserId.Returns(1);

        return new ReviewAnswersViewBuilder(_logger, _contentful, _submissions, _currentUser);
    }

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
    {
        var submission = new SubmissionResponsesModel(123,
            answerIds.Select(a => new QuestionWithAnswerModel { AnswerSysId = a }).ToList())
        {
            DateCompleted = completed,
            Establishment = new SqlEstablishmentDto
            {
                OrgName = "Test Trust"
            }
        };

        return new SubmissionRoutingDataModel(next, section, submission, status);
    }

    // ---------------- RouteToCheckAnswers ----------------

    [Fact]
    public async Task RouteToCheckAnswers_NotStarted_Redirects_Home()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.ActiveEstablishmentId.Returns(1);
        var section = MakeSection("S1", "sec-1");
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        _submissions.GetSubmissionRoutingDataAsync(1, section, false)
                    .Returns(MakeRouting(SubmissionStatus.NotStarted, section));

        var result = await sut.RouteToCheckAnswers(ctl, "cat", "sec-1");
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Theory]
    [InlineData(SubmissionStatus.InProgress)]
    [InlineData(SubmissionStatus.CompleteNotReviewed)]
    [InlineData(SubmissionStatus.CompleteReviewed)]
    public async Task RouteToCheckAnswers_Shows_CheckAnswers_For_Allowed_Statuses(SubmissionStatus status)
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.ActiveEstablishmentId.Returns(1);
        var section = MakeSection("S1", "sec-1", "Section 1");
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        _contentful.GetPageBySlugAsync(UrlConstants.CheckAnswersSlug)
                   .Returns(new PageEntry { Content = new List<ContentfulEntry> { new PageEntry { Sys = new SystemDetails("X") } } });

        _submissions.GetSubmissionRoutingDataAsync(1, section, false)
                    .Returns(MakeRouting(status, section, next: MakeQuestion("Q2", "q-2")));

        var result = await sut.RouteToCheckAnswers(ctl, "cat", "sec-1", errorMessage: "err");
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ReviewAnswersViewBuilder.CheckAnswersViewName, view.ViewName);

        var vm = Assert.IsType<ReviewAnswersViewModel>(view.Model);
        Assert.Equal("Section 1", vm.SectionName);
        Assert.Equal("cat", vm.CategorySlug);
        Assert.Equal("sec-1", vm.SectionSlug);
        Assert.Equal("err", vm.ErrorMessage);
        Assert.NotEmpty(vm.Content);
    }

    [Fact]
    public async Task RouteToCheckAnswers_Default_Falls_Back_To_Redirect_To_Next_Question()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.ActiveEstablishmentId.Returns(1);
        var section = MakeSection("S1", "sec-1");
        _contentful.GetSectionBySlugAsync("sec-1").Returns(section);

        var weirdStatus = (SubmissionStatus)999;
        var routing = MakeRouting(SubmissionStatus.InProgress, section, next: MakeQuestion("Q1", "q-1"));
        typeof(SubmissionRoutingDataModel).GetProperty(nameof(SubmissionRoutingDataModel.Status))!
            .SetValue(routing, weirdStatus);

        _submissions.GetSubmissionRoutingDataAsync(1, section, false).Returns(routing);

        var result = await sut.RouteToCheckAnswers(ctl, "cat", "sec-1");
        Assert.IsType<RedirectToActionResult>(result);
    }

    // ---------------- RouteToViewAnswers ----------------

    [Fact]
    public async Task RouteToViewAnswers_NotStarted_Redirects_Home()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.ActiveEstablishmentId.Returns(2);
        var section = MakeSection("S2", "sec-2");
        _contentful.GetSectionBySlugAsync("sec-2").Returns(section);

        _submissions.GetSubmissionRoutingDataAsync(2, section, true)
                    .Returns(MakeRouting(SubmissionStatus.NotStarted, section));

        var result = await sut.RouteToViewAnswers(ctl, "cat", "sec-2");
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task RouteToViewAnswers_InProgress_Redirects_To_ContinueSelfAssessment()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.ActiveEstablishmentId.Returns(2);
        var section = MakeSection("S2", "sec-2");
        _contentful.GetSectionBySlugAsync("sec-2").Returns(section);

        _submissions.GetSubmissionRoutingDataAsync(2, section, true)
                    .Returns(MakeRouting(SubmissionStatus.InProgress, section));

        var result = await sut.RouteToViewAnswers(ctl, "cat", "sec-2");
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task RouteToViewAnswers_CompleteNotReviewed_Redirects_To_ContinueSelfAssessment()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.ActiveEstablishmentId.Returns(2);
        var section = MakeSection("S2", "sec-2");
        _contentful.GetSectionBySlugAsync("sec-2").Returns(section);

        _submissions.GetSubmissionRoutingDataAsync(2, section, true)
                    .Returns(MakeRouting(SubmissionStatus.CompleteNotReviewed, section));

        var result = await sut.RouteToViewAnswers(ctl, "cat", "sec-2");
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task RouteToViewAnswers_CompleteReviewed_Shows_ViewAnswers()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.ActiveEstablishmentId.Returns(77);
        var section = MakeSection("S7", "sec-7", "Section 7");
        _contentful.GetSectionBySlugAsync("sec-7").Returns(section);

        // Build routing with Establishment initialised
        var routing = new SubmissionRoutingDataModel(
            nextQuestion: null,
            questionnaireSection: section,
            submission: new SubmissionResponsesModel(123, [])
            {
                DateCompleted = DateTime.UtcNow,
                Establishment = new SqlEstablishmentDto { OrgName = "Test Trust" }
            },
            status: SubmissionStatus.CompleteReviewed
        );

        _submissions.GetSubmissionRoutingDataAsync(77, section, true).Returns(routing);

        // Act
        var result = await sut.RouteToViewAnswers(ctl, "cat", "sec-7");

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(ReviewAnswersViewBuilder.ViewAnswersViewName, view.ViewName);

        var vm = Assert.IsType<ViewAnswersViewModel>(view.Model);
        Assert.Equal("Section 7", vm.TopicName);
        Assert.Equal("cat", vm.CategorySlug);
        Assert.Equal("sec-7", vm.SectionSlug);
    }


    [Fact]
    public async Task RouteToViewAnswers_Default_Redirects_To_GetQuestionBySlug()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        _currentUser.ActiveEstablishmentId.Returns(2);
        var q1 = MakeQuestion("QX", "q-x");
        var section = MakeSection("S2", "sec-2", "S", q1);
        _contentful.GetSectionBySlugAsync("sec-2").Returns(section);

        var routing = MakeRouting((SubmissionStatus)999, section, next: q1);
        _submissions.GetSubmissionRoutingDataAsync(2, section, true).Returns(routing);

        var result = await sut.RouteToViewAnswers(ctl, "cat", "sec-2");
        Assert.IsType<RedirectToActionResult>(result);
    }

    // ---------------- ConfirmCheckAnswers ----------------

    [Fact]
    public async Task ConfirmCheckAnswers_Success_Redirects_To_Category_Landing_And_Sets_SectionName()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        var section = MakeSection("S1", "sec", "My Section");
        _contentful.GetSectionBySlugAsync("sec").Returns(section);
        _submissions.ConfirmCheckAnswersAndUpdateRecommendationsAsync(
            Arg.Any<int>(),
            Arg.Any<int?>(),
            42,
            Arg.Any<int>(),
            Arg.Any<QuestionnaireSectionEntry>()).Returns(Task.CompletedTask);

        var result = await sut.ConfirmCheckAnswers(ctl, "cat", "sec", "My Section", 42);

        await _submissions.Received(1).ConfirmCheckAnswersAndUpdateRecommendationsAsync(
            2, // ActiveEstablishmentId
            null, // UserOrganisationId (non-MAT user)
            42,
            1, // UserId
            Arg.Is<QuestionnaireSectionEntry>(s => s.Sys.Id == "S1"));

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.True(ctl.TempData.ContainsKey("SectionName"));
        Assert.Equal("My Section", ctl.TempData["SectionName"]);
    }

    [Fact]
    public async Task ConfirmCheckAnswers_Failure_Sets_Error_TempData_And_Redirects_Back_To_CheckAnswers()
    {
        var sut = CreateSut();
        var ctl = MakeController();

        var section = MakeSection("S1", "sec", "S");
        _contentful.GetSectionBySlugAsync("sec").Returns(section);
        _submissions.ConfirmCheckAnswersAndUpdateRecommendationsAsync(
            Arg.Any<int>(),
            Arg.Any<int?>(),
            9,
            Arg.Any<int>(),
            Arg.Any<QuestionnaireSectionEntry>())
            .ThrowsAsync(new Exception("boom"));

        var result = await sut.ConfirmCheckAnswers(ctl, "cat", "sec", "S", 9);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.True(ctl.TempData.ContainsKey("ErrorMessage"));
        Assert.Equal(ReviewAnswersViewBuilder.InlineRecommendationUnavailableErrorMessage, ctl.TempData["ErrorMessage"]);
    }

    private sealed class DummyController : Controller { }
}
