using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Response.Queries;
using Dfe.PlanTech.Application.Submission.Commands;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class CheckAnswersControllerTests
{
    private readonly CheckAnswersController _checkAnswersController;

    private readonly Mock<IPlanTechDbContext> _planTechDbContextMock;

    private readonly Mock<IGetQuestionQuery> _getQuestionQueryMock;
    private readonly Mock<IGetAnswerQuery> _getAnswerQueryMock;
    private readonly Mock<IContentRepository> _contentRepositoryMock;

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

    public CheckAnswersControllerTests()
    {

        Mock<ILogger<CheckAnswersController>> loggerMock = new Mock<ILogger<CheckAnswersController>>();
        Mock<IUrlHistory> urlHistoryMock = new Mock<IUrlHistory>();

        _planTechDbContextMock = new Mock<IPlanTechDbContext>();

        Mock<IQuestionnaireCacher> questionnaireCacherMock = new Mock<IQuestionnaireCacher>();
        _contentRepositoryMock = SetupRepositoryMock();

        ICalculateMaturityCommand calculateMaturityCommand = new CalculateMaturityCommand(_planTechDbContextMock.Object);
        IGetResponseQuery getResponseQuery = new GetResponseQuery(_planTechDbContextMock.Object);
        _getQuestionQueryMock = new Mock<IGetQuestionQuery>();
        _getAnswerQueryMock = new Mock<IGetAnswerQuery>();
        GetPageQuery getPageQuery = new GetPageQuery(questionnaireCacherMock.Object, _contentRepositoryMock.Object);

        _checkAnswersController = new CheckAnswersController
        (
            loggerMock.Object,
            urlHistoryMock.Object,
            calculateMaturityCommand,
            getResponseQuery,
            _getQuestionQueryMock.Object,
            _getAnswerQueryMock.Object,
            getPageQuery
        );
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
        _getQuestionQueryMock.Setup(m => m.GetQuestionBy(1)).ReturnsAsync(new Domain.Questions.Models.Question() { QuestionText = "Question Text" });
        _getAnswerQueryMock.Setup(m => m.GetAnswerBy(1)).ReturnsAsync(new Domain.Answers.Models.Answer() { AnswerText = "Answer Text" });

        var result = await _checkAnswersController.CheckAnswersPage(SubmissionId);

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
        Assert.Equal("Question Text", checkAnswerDto.QuestionAnswerList[0].QuestionText);
        Assert.Equal("Answer Text", checkAnswerDto.QuestionAnswerList[0].AnswerText);
    }

    [Fact]
    public async Task CheckAnswersPage_ThrowsException_When_ResponseList_IsNull()
    {
        Response[]? responseList = null;

        _planTechDbContextMock.Setup(m => m.GetResponseListBy(SubmissionId)).ReturnsAsync(responseList);

        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(SubmissionId));
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
        _getQuestionQueryMock.Setup(m => m.GetQuestionBy(1)).ReturnsAsync(new Domain.Questions.Models.Question() { QuestionText = null });

        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(SubmissionId));
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
        _getQuestionQueryMock.Setup(m => m.GetQuestionBy(1)).ReturnsAsync(new Domain.Questions.Models.Question() { QuestionText = "Question Text" });
        _getAnswerQueryMock.Setup(m => m.GetAnswerBy(1)).ReturnsAsync(new Domain.Answers.Models.Answer() { AnswerText = null });

        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _checkAnswersController.CheckAnswersPage(SubmissionId));
    }
}