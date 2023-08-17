using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers;

public class QuestionsControllerTests
{
    private const int COMPLETED_SUBMISSION_ID = 1;
    private const string COMPLETED_SECTION_ID = "CompletedSectionId";

    private const int INCOMPLETE_SUBMISSION_ID = 2;
    private const string INCOMPLETE_SECTION_ID = "IncompleteSectionId";
    private const string FIRST_QUESTION_ID = "Question1";
    private const string FIRST_ANSWER_ID = "Answer1";
    private const string SECOND_QUESTION_ID = "Question2";

    private readonly List<Submission> _submissions = new()
            {
                new Submission()
                {
                    Id = INCOMPLETE_SUBMISSION_ID,
                    EstablishmentId = 0,
                    Completed = false,
                    SectionId = INCOMPLETE_SECTION_ID,
                    SectionName = "SectionName",
                    Maturity = null,
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = DateTime.UtcNow,
                    DateCompleted = null
                },
                new Submission()
                {
                    Id = COMPLETED_SUBMISSION_ID,
                    EstablishmentId = 0,
                    Completed = true,
                    SectionId = COMPLETED_SECTION_ID,
                    SectionName = "SectionName",
                    Maturity = Maturity.Medium.ToString(),
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = DateTime.UtcNow,
                    DateCompleted = DateTime.UtcNow
                }
            };

    private readonly List<Question> _questions = new() {
           new Question()
           {
               Sys = new SystemDetails(){
                   Id = FIRST_QUESTION_ID
               },
               Text = "Question One",
               HelpText = "Explanation",
               Answers = new[] {
                   new Answer(){
                       Sys = new SystemDetails() { Id = FIRST_ANSWER_ID },
                       NextQuestion = new Question() { Sys = new SystemDetails() { Id = SECOND_QUESTION_ID } },
                       Text = "Question 1 - Answer 1"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer2" },
                       NextQuestion = new Question() { Sys = new SystemDetails() { Id = SECOND_QUESTION_ID } },
                       Text = "Question 1 - Answer 2"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer3" },
                       NextQuestion = new Question() { Sys = new SystemDetails() { Id = SECOND_QUESTION_ID } },
                       Text = "Question 1 - Answer 3"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer4" },
                       NextQuestion = new Question() { Sys = new SystemDetails() { Id = SECOND_QUESTION_ID } },
                       Text = "Question 1 - Answer 4"
                   }
               }
           },
           new Question()
           {
               Sys = new SystemDetails(){
                   Id = SECOND_QUESTION_ID
               },
               Text = "Question Two",
               HelpText = "Explanation",
               Answers = new[] {
                   new Answer(){
                       Sys = new SystemDetails() { Id = FIRST_ANSWER_ID },
                       NextQuestion = null,
                       Text = "Question 2 - Answer 1"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer2" },
                       NextQuestion = null,
                       Text = "Question 2 - Answer 2"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer3" },
                       NextQuestion = null,
                       Text = "Question 2 - Answer 3"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer4" },
                       NextQuestion = null,
                       Text = "Question 2 - Answer 4"
                   }
               }
           }

       };

    private readonly QuestionsController _controller;
    private readonly ISubmitAnswerCommand _submitAnswerCommand;

    public QuestionsControllerTests()
    {
        var mockLogger = Substitute.For<ILogger<QuestionsController>>();

        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());
        tempData["param"] = "admin";

        _submitAnswerCommand = Substitute.For<ISubmitAnswerCommand>();

        _submitAnswerCommand.GetNextQuestionId(Arg.Any<string>(), Arg.Any<string>()).Returns((callInfo) =>
        {
            var questionId = callInfo.ArgAt<string>(0);
            var chosenAnswerId = callInfo.ArgAt<string>(1);

            var nextQuestionId = _questions.Where(question => question.Sys.Id == questionId)
                                            .Select(question => question.Answers.FirstOrDefault(answer => answer.Sys.Id.Equals(chosenAnswerId)))
                                            .Where(answer => answer?.NextQuestion?.Sys.Id != null)
                                            .Select(answer => answer!.NextQuestion!.Sys.Id)
                                            .FirstOrDefault();

            return Task.FromResult(nextQuestionId);
        });

        _submitAnswerCommand.GetOngoingSubmission(Arg.Any<string>()).Returns((callinfo) =>
        {
            var sectionId = callinfo.ArgAt<string>(0);

            var submission = _submissions.FirstOrDefault(submission => submission.SectionId == sectionId);

            return Task.FromResult(submission);
        });

        _submitAnswerCommand.GetQuestionnaireQuestion(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                            .Returns((callInfo) =>
                            {
                                var questionId = callInfo.ArgAt<string>(0);
                                var sectionId = callInfo.ArgAt<string>(1);

                                var matchingQuestion = _questions.First(q => q.Sys.Id == questionId);

                                return Task.FromResult(matchingQuestion);
                            });

        _submitAnswerCommand.SubmitAnswer(Arg.Any<SubmitAnswerDto>(), Arg.Any<string>(), Arg.Any<string>())
        .Returns((callInfo) =>
        {
            var sectionId = callInfo.ArgAt<string>(1);

            var submission = _submissions.Where(submission => submission.SectionId == sectionId)
                                        .Select(submission => submission.Id)
                                        .FirstOrDefault();

            return Task.FromResult(submission);
        });

        _submitAnswerCommand.NextQuestionIsAnswered(Arg.Any<int>(), Arg.Any<string>())
                            .Returns((callInfo) =>
                            {
                                var submissionId = callInfo.ArgAt<int>(0);
                                var questionId = callInfo.ArgAt<string>(1);

                                var submission = _submissions.Where(submission => submission.Id == submissionId)
                                        .Select(submission => submission.Completed)
                                        .FirstOrDefault();

                                return submission;
                            });

        _controller = new QuestionsController(mockLogger) { TempData = tempData, ControllerContext = ControllerHelpers.MockControllerContext() };
    }

    [Fact]
    public async Task GetQuestionById_Should_ReturnQuestionPage_When_FetchingQuestionWithValidId()
    {
        var id = FIRST_QUESTION_ID;

        var result = await _controller.GetQuestionById(id, null, 1, null, _submitAnswerCommand, CancellationToken.None);
        Assert.IsType<ViewResult>(result);

        var viewResult = result as ViewResult;

        var model = viewResult!.Model;

        Assert.IsType<QuestionViewModel>(model);

        var question = model as QuestionViewModel;

        Assert.NotNull(question);
        Assert.Equal("Question One", question.Question.Text);
    }

    [Fact]
    public async Task GetQuestionById_Should_ReturnAsNormal_If_Submission_IsNull()
    {
        _controller.TempData["param"] = "SectionName+SectionId";

        var questionRef = FIRST_QUESTION_ID;


        var result = await _controller.GetQuestionById(questionRef, null, null, null, _submitAnswerCommand, CancellationToken.None);
        Assert.IsType<ViewResult>(result);

        var viewResult = result as ViewResult;

        var model = viewResult!.Model;

        Assert.IsType<QuestionViewModel>(model);

        var question = model as QuestionViewModel;

        Assert.NotNull(question);
        Assert.Equal(FIRST_QUESTION_ID, question.Question.Sys.Id);
    }

    [Fact]
    public async Task GetQuestionById_Should_ReturnAsNormal_If_PastSubmission_IsComplete()
    {
        _controller.TempData["param"] = "SectionName+SectionId";

        var questionRef = FIRST_QUESTION_ID;

        var result = await _controller.GetQuestionById(questionRef, null, null, null, _submitAnswerCommand, CancellationToken.None);
        Assert.IsType<ViewResult>(result);

        var viewResult = result as ViewResult;

        var model = viewResult!.Model;

        Assert.IsType<QuestionViewModel>(model);

        var question = model as QuestionViewModel;

        Assert.NotNull(question);
        Assert.Equal(FIRST_QUESTION_ID, question.Question.Sys.Id);
    }

    [Fact]
    public async Task GetQuestionById_Should_ReturnUnansweredQuestion_If_PastSubmission_IsNotCompleted()
    {
        _controller.TempData["param"] = "SectionName+SectionId";

        var questionRef = FIRST_QUESTION_ID;

        var result = await _controller.GetQuestionById(questionRef, null, null, null, _submitAnswerCommand, CancellationToken.None);
        Assert.IsType<ViewResult>(result);

        var viewResult = result as ViewResult;

        var model = viewResult!.Model;

        Assert.IsType<QuestionViewModel>(model);

        var question = model as QuestionViewModel;

        Assert.NotNull(question);
        Assert.Equal(FIRST_QUESTION_ID, question.Question.Sys.Id);
    }

    [Fact]
    public async Task GetQuestionById_Should_RedirectToCheckAnswersController_If_PastSubmission_IsNotCompleted_And_ThereIsNo_NextQuestion()
    {
        SetParams(INCOMPLETE_SECTION_ID);

        var questionRef = SECOND_QUESTION_ID;

        var result = await _controller.GetQuestionById(questionRef, null, null, null, _submitAnswerCommand, CancellationToken.None);
        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("CheckAnswers", redirectToActionResult.ControllerName);
        Assert.Equal("CheckAnswersPage", redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "submissionId");
        Assert.Equal(INCOMPLETE_SUBMISSION_ID, id.Value);
    }

    [Fact]
    public async Task GetQuestionById_Should_ThrowException_When_IdIsNull()
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionById(null!, null, null, null, _submitAnswerCommand, CancellationToken.None));
    }

    [Fact]
    public void SubmitAnswer_Should_ThrowException_When_NullArgument()
    {
        Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.SubmitAnswer(null!, _submitAnswerCommand));
    }

    [Fact]
    public async void SubmitAnswer_Should_RedirectToNextQuestion_When_NextQuestionId_Exists()
    {
        var submitAnswerDto = new SubmitAnswerDto()
        {
            QuestionId = FIRST_QUESTION_ID,
            ChosenAnswerId = FIRST_ANSWER_ID,
        };

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
        Assert.Equal(SECOND_QUESTION_ID, id.Value);
    }

    [Fact]
    public async void SubmitAnswer_Should_RedirectTo_CheckAnswers_When_NextQuestionId_IsNull()
    {
        var submitAnswerDto = new SubmitAnswerDto()
        {
            QuestionId = SECOND_QUESTION_ID,
            ChosenAnswerId = FIRST_ANSWER_ID,
            SubmissionId = COMPLETED_SUBMISSION_ID,
            Params = CreateParams(COMPLETED_SECTION_ID)
        };

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("CheckAnswers", redirectToActionResult.ControllerName);
        Assert.Equal("CheckAnswersPage", redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "submissionId");
        Assert.Equal(submitAnswerDto.SubmissionId, id.Value);
    }

    [Fact]
    public async void SubmitAnswer_Should_RedirectTo_CheckAnswers_When_NextQuestionIsAnswered()
    {

        var submitAnswerDto = new SubmitAnswerDto()
        {
            QuestionId = FIRST_QUESTION_ID,
            ChosenAnswerId = FIRST_ANSWER_ID,
            SubmissionId = COMPLETED_SUBMISSION_ID,
            Params = CreateParams(COMPLETED_SECTION_ID)
        };

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("CheckAnswers", redirectToActionResult.ControllerName);
        Assert.Equal("CheckAnswersPage", redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "submissionId");
        Assert.Equal(submitAnswerDto.SubmissionId, id.Value);
    }

    [Fact]
    public async void SubmitAnswer_Should_RedirectTo_SameQuestion_When_ChosenAnswerId_IsNull()
    {
        var submitAnswerDto = new SubmitAnswerDto()
        {
            QuestionId = FIRST_QUESTION_ID,
            ChosenAnswerId = null!,
        };

        _controller.ModelState.AddModelError("ChosenAnswerId", "Required");

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
        Assert.Equal(submitAnswerDto.QuestionId, id.Value);
    }

    [Fact]
    public async void SubmitAnswer_Should_RedirectTo_SameQuestion_When_NextQuestionId_And_ChosenAnswerId_IsNull()
    {
        var submitAnswerDto = new SubmitAnswerDto()
        {
            QuestionId = FIRST_QUESTION_ID,
            ChosenAnswerId = null!
        };

        _controller.ModelState.AddModelError("ChosenAnswerId", "Required");

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
        Assert.Equal(submitAnswerDto.QuestionId, id.Value);
    }

    [Fact]
    public async void SubmitAnswer_Params_Should_Parse_When_Params_IsNotNull()
    {
        var submitAnswerDto = new SubmitAnswerDto()
        {
            QuestionId = FIRST_QUESTION_ID,
            ChosenAnswerId = FIRST_ANSWER_ID,
            Params = "SectionName+SectionId"
        };

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
        Assert.NotNull(redirectToActionResult.RouteValues);
        var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
        Assert.Equal(SECOND_QUESTION_ID, id.Value);
    }

    private void SetParams(string sectionId)
    {
        _controller.TempData["param"] = CreateParams(sectionId);
    }

    private static string CreateParams(string sectionId) => $"SectionName+{sectionId}";
}
