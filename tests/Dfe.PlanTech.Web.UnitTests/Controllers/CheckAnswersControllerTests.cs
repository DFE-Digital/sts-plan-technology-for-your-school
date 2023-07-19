using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Response.Queries;
using Dfe.PlanTech.Application.Submission.Commands;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class CheckAnswersControllerTests
{
    private readonly CheckAnswersController _checkAnswersController;

    private readonly Mock<IPlanTechDbContext> _planTechDbContextMock;

    private readonly Mock<IContentRepository> _contentRepositoryMock;

    private readonly Mock<ICalculateMaturityCommand> _calculateMaturityCommandMock;

    private Page[] _pages = new Page[]
    {
        new Page()
        {
            Slug = "check-answers",
            Title = new Title() { Text = "Title Text" },
            Content = new ContentComponent[] { new Header() { Tag = Domain.Content.Enums.HeaderTag.H1, Text = "Header Text" }}
        },
    };

    private const int SubmissionId = 1;
    private const string sectionName = "Section Name";

    public CheckAnswersControllerTests()
    {

        Mock<ILogger<CheckAnswersController>> loggerMock = new Mock<ILogger<CheckAnswersController>>();
        Mock<IUrlHistory> urlHistoryMock = new Mock<IUrlHistory>();

        _planTechDbContextMock = new Mock<IPlanTechDbContext>();

        Mock<IQuestionnaireCacher> questionnaireCacherMock = new Mock<IQuestionnaireCacher>();
        _contentRepositoryMock = SetupRepositoryMock();

        _calculateMaturityCommandMock = new Mock<ICalculateMaturityCommand>();
        IGetResponseQuery getResponseQuery = new GetResponseQuery(_planTechDbContextMock.Object);
        IGetQuestionQuery getQuestionQuery = new GetQuestionQuery(_planTechDbContextMock.Object);
        IGetAnswerQuery getAnswerQuery = new GetAnswerQuery(_planTechDbContextMock.Object);
        GetPageQuery getPageQuery = new GetPageQuery(questionnaireCacherMock.Object, _contentRepositoryMock.Object);

        Mock<ITempDataDictionary> tempDataMock = new Mock<ITempDataDictionary>();
        
        _checkAnswersController = new CheckAnswersController
        (
            loggerMock.Object,
            urlHistoryMock.Object,
            _calculateMaturityCommandMock.Object,
            getResponseQuery,
            getQuestionQuery,
            getAnswerQuery,
            getPageQuery
        );

        _checkAnswersController.TempData = tempDataMock.Object;
    }

    private Mock<IContentRepository> SetupRepositoryMock()
    {
        var repositoryMock = new Mock<IContentRepository>();
        repositoryMock.Setup(repo => repo.GetEntities<Page>(It.IsAny<IGetEntitiesOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync((IGetEntitiesOptions options, CancellationToken _) =>
        {
            if (options?.Queries != null)
            {
                foreach (var query in options.Queries)
                {
                    if (query is ContentQueryEquals equalsQuery && query.Field == "fields.slug")
                    {
                        return _pages.Where(page => page.Slug == equalsQuery.Value);
                    }
                }
            }
            return Array.Empty<Page>();
        });
        return repositoryMock;
    }

    [Fact]
    public async Task CheckAnswersPage_RedirectsTo_View_When_CheckAnswersViewModel_IsPopulated()
    {
        Response[]? responseList = new Response[]
        {
            new Response()
            {
                SubmissionId = SubmissionId,
                QuestionId = 1,
                AnswerId = 1,
            }
        };

        _planTechDbContextMock.Setup(m => m.GetResponseListBy(SubmissionId)).ReturnsAsync(responseList);
        _planTechDbContextMock.Setup(m => m.GetQuestionBy(1)).ReturnsAsync(new Domain.Questions.Models.Question() { ContentfulRef = "QuestionRef", QuestionText = "Question Text" });
        _planTechDbContextMock.Setup(m => m.GetAnswerBy(1)).ReturnsAsync(new Domain.Answers.Models.Answer() { ContentfulRef = "AnswerRef", AnswerText = "Answer Text" });

        var result = await _checkAnswersController.CheckAnswersPage(SubmissionId, sectionName);

        Assert.IsType<ViewResult>(result);

        var viewResult = result as ViewResult;

        Assert.NotNull(viewResult);
        Assert.Equal("CheckAnswers", viewResult.ViewName);

        Assert.IsType<CheckAnswersViewModel>(viewResult.Model);

        var checkAnswersViewModel = viewResult.Model as CheckAnswersViewModel;

        Assert.NotNull(checkAnswersViewModel);
        Assert.Equal("self-assessment", checkAnswersViewModel.BackUrl);
        Assert.Equal("Title Text", checkAnswersViewModel.Title.Text);
        Assert.Equal(SubmissionId, checkAnswersViewModel.SubmissionId);

        Assert.IsType<Header>(checkAnswersViewModel.Content[0]);

        var content = checkAnswersViewModel.Content[0] as Header;

        Assert.NotNull(content);
        Assert.Equal("Header Text", content.Text);

        Assert.IsType<CheckAnswerDto>(checkAnswersViewModel.CheckAnswerDto);

        var checkAnswerDto = checkAnswersViewModel.CheckAnswerDto;

        Assert.NotNull(checkAnswerDto);
        Assert.Equal("QuestionRef", checkAnswerDto.QuestionAnswerList[0].QuestionRef);
        Assert.Equal("Question Text", checkAnswerDto.QuestionAnswerList[0].QuestionText);
        Assert.Equal("AnswerRef", checkAnswerDto.QuestionAnswerList[0].AnswerRef);
        Assert.Equal("Answer Text", checkAnswerDto.QuestionAnswerList[0].AnswerText);
    }

    [Fact]
    public void CheckAnswersPage_RedirectsTo_View_When_ChangeAnswer_IsCalled()
    {
        Domain.Questions.Models.Question question = new Domain.Questions.Models.Question() { ContentfulRef = "QuestionRef", QuestionText = "Question Text" };
        Domain.Answers.Models.Answer answer = new Domain.Answers.Models.Answer() { ContentfulRef = "Answer", AnswerText = "Question Text" };

        var result = _checkAnswersController.ChangeAnswer(question.ContentfulRef, answer.ContentfulRef, SubmissionId);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
        Assert.Equal(question.ContentfulRef, id.Value);
        var answerRef = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "answerRef");
        Assert.Equal(answer.ContentfulRef, answerRef.Value);
    }

    [Fact]
    public async Task CheckAnswersPage_ThrowsException_When_ResponseList_IsNull()
    {
        Response[]? responseList = null;

        _planTechDbContextMock.Setup(m => m.GetResponseListBy(SubmissionId)).ReturnsAsync(responseList);

        await Assert.ThrowsAnyAsync<NullReferenceException>(() => _checkAnswersController.CheckAnswersPage(SubmissionId, sectionName));
    }

    [Fact]
    public async Task CheckAnswersPage_ThrowsException_When_QuestionText_IsNull()
    {
        Response[]? responseList = new Response[]
        {
            new Response()
            {
                SubmissionId = SubmissionId,
                QuestionId = 1,
                AnswerId = 1,
            }
        };

        _planTechDbContextMock.Setup(m => m.GetResponseListBy(SubmissionId)).ReturnsAsync(responseList);
        _planTechDbContextMock.Setup(m => m.GetQuestionBy(1)).ReturnsAsync(new Domain.Questions.Models.Question() { QuestionText = null });

        await Assert.ThrowsAnyAsync<NullReferenceException>(() => _checkAnswersController.CheckAnswersPage(SubmissionId, sectionName));
    }

    [Fact]
    public async Task CheckAnswersPage_ThrowsException_When_AnswerText_IsNull()
    {
        Response[]? responseList = new Response[]
        {
            new Response()
            {
                SubmissionId = SubmissionId,
                QuestionId = 1,
                AnswerId = 1,
            }
        };

        _planTechDbContextMock.Setup(m => m.GetResponseListBy(SubmissionId)).ReturnsAsync(responseList);
        _planTechDbContextMock.Setup(m => m.GetQuestionBy(1)).ReturnsAsync(new Domain.Questions.Models.Question() { QuestionText = "Question Text" });
        _planTechDbContextMock.Setup(m => m.GetAnswerBy(1)).ReturnsAsync(new Domain.Answers.Models.Answer() { AnswerText = null });

        await Assert.ThrowsAnyAsync<NullReferenceException>(() => _checkAnswersController.CheckAnswersPage(SubmissionId, sectionName));
    }

    [Fact]
    public async Task ConfirmCheckAnswers_RedirectsToSelfAssessment_WhenMaturityIsLargerThan1()
    {
        _calculateMaturityCommandMock.Setup(m => m.CalculateMaturityAsync(It.IsAny<int>())).ReturnsAsync(2);

        var result = await _checkAnswersController.ConfirmCheckAnswers(It.IsAny<int>(), "Test");

        Assert.IsType<RedirectToActionResult>(result);

        var res = result as RedirectToActionResult;

        if (res != null)
        {
            Assert.True(res.ActionName == "GetByRoute");
            Assert.True(res.ControllerName == "Pages");
        }
    }

    [Fact]
    public async Task ConfirmCheckAnswers_ReturnsNull_WhenMaturityIsLessThan1()
    {
        _calculateMaturityCommandMock.Setup(m => m.CalculateMaturityAsync(It.IsAny<int>())).ReturnsAsync(0);

        var result = await _checkAnswersController.ConfirmCheckAnswers(It.IsAny<int>(), "Test");

        Assert.Null(result);
    }
}