using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Response.Queries;
using Dfe.PlanTech.Application.Submission.Commands;
using Dfe.PlanTech.Application.Submission.Interface;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Constants;
using Dfe.PlanTech.Domain.Questionnaire.Enums;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
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

    private IPlanTechDbContext _databaseSubstitute;
    private readonly QuestionsController _controller;
    private readonly SubmitAnswerCommand _submitAnswerCommand;
    private IQuestionnaireCacher _questionnaireCacherSubstitute;
    private IGetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQuerySubstitute;
    private readonly ICacher _cacher;

    public QuestionsControllerTests()
    {
        IContentRepository repositorySubstitute = SubstituteRepository();
        _questionnaireCacherSubstitute = SubstituteQuestionnaireCacher();

        var Logger = Substitute.For<ILogger<QuestionsController>>();
        _databaseSubstitute = Substitute.For<IPlanTechDbContext>();
        var user = Substitute.For<IUser>();

        var getQuestionnaireQuery = new Application.Questionnaire.Queries.GetQuestionQuery(_questionnaireCacherSubstitute, repositorySubstitute);

        ICreateQuestionCommand createQuestionCommand = new CreateQuestionCommand(_databaseSubstitute);
        IRecordQuestionCommand recordQuestionCommand = new RecordQuestionCommand(_databaseSubstitute);

        IGetQuestionQuery getQuestionQuery = new Application.Submission.Queries.GetQuestionQuery(_databaseSubstitute);
        ICreateAnswerCommand createAnswerCommand = new CreateAnswerCommand(_databaseSubstitute);
        IRecordAnswerCommand recordAnswerCommand = new RecordAnswerCommand(_databaseSubstitute);
        ICreateResponseCommand createResponseCommand = new CreateResponseCommand(_databaseSubstitute);
        IGetResponseQuery getResponseQuery = new GetResponseQuery(_databaseSubstitute);
        IGetSubmissionQuery getSubmissionQuery = new GetSubmissionQuery(_databaseSubstitute);
        ICreateSubmissionCommand createSubmissionCommand = new CreateSubmissionCommand(_databaseSubstitute);
        _getLatestResponseListForSubmissionQuerySubstitute = Substitute.For<IGetLatestResponseListForSubmissionQuery>();

        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());
        tempData["param"] = "admin";

        GetSubmitAnswerQueries getSubmitAnswerQueries = new GetSubmitAnswerQueries(getQuestionQuery, getResponseQuery, getSubmissionQuery, getQuestionnaireQuery, user);
        RecordSubmitAnswerCommands recordSubmitAnswerCommands = new RecordSubmitAnswerCommands(recordQuestionCommand, recordAnswerCommand, createSubmissionCommand, createResponseCommand);

        _submitAnswerCommand = new SubmitAnswerCommand(getSubmitAnswerQueries, recordSubmitAnswerCommands, _getLatestResponseListForSubmissionQuerySubstitute);

        _controller = new QuestionsController(Logger) { TempData = tempData };

        _cacher = new Cacher(new CacheOptions(), new MemoryCache(new MemoryCacheOptions()));
    }

    private static IQuestionnaireCacher SubstituteQuestionnaireCacher()
    {
        var substitute = Substitute.For<IQuestionnaireCacher>();
        substitute.Cached.Returns(new QuestionnaireCache());
        substitute.When(x => x.SaveCache(Arg.Any<QuestionnaireCache>()));

        return substitute;
    }

    private IContentRepository SubstituteRepository()
    {
        var repositorySubstitute = Substitute.For<IContentRepository>();
        repositorySubstitute.GetEntities<Question>(Arg.Any<IGetEntitiesOptions>(), Arg.Any<CancellationToken>()).Returns((callInfo) =>
        {
            IGetEntitiesOptions options = (IGetEntitiesOptions)callInfo[0];
            if (options?.Queries != null)
            {
                foreach (var query in options.Queries)
                {
                    if (query is ContentQueryEquals equalsQuery && query.Field == "sys.id")
                    {
                        return _questions.Where(question => question.Sys.Id == equalsQuery.Value);
                    }
                }
            }

            return Array.Empty<Question>();
        });

        repositorySubstitute.GetEntityById<Question>(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                      .Returns((callInfo) => 
                      {
                          string id = (string)callInfo[0];
                          return Task.FromResult(_questions.FirstOrDefault(question => question.Sys.Id == id));
                      });
        return repositorySubstitute;
    }

    [Fact]
    public async Task GetQuestionById_Should_ReturnQuestionPage_When_FetchingQuestionWithValidId()
    {
        var id = FIRST_QUESTION_ID;

        var result = await _controller.GetQuestionById(id, null, _submitAnswerCommand, CancellationToken.None);
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

        var questionRef = "Question1";

        List<Submission> submissionList = new List<Submission>()
            {
                new Submission()
                {
                    Id = 1,
                    EstablishmentId = -1,
                    Completed = true,
                    SectionId = "",
                    SectionName = "",
                    Maturity = null,
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = null,
                    DateCompleted = null
                }
            };

        var query = Arg.Any<IQueryable<Submission>>();
        _databaseSubstitute.FirstOrDefaultAsync(query).Returns(submissionList[0]);

        _controller.TempData[TempDataConstants.Questions] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataQuestions() { QuestionRef = questionRef, AnswerRef = null, SubmissionId = null });

        var result = await _controller.GetQuestionById(null, null, _submitAnswerCommand, CancellationToken.None);
        Assert.IsType<ViewResult>(result);

        var viewResult = result as ViewResult;

        var model = viewResult!.Model;

        Assert.IsType<QuestionViewModel>(model);

        var question = model as QuestionViewModel;

        Assert.NotNull(question);
        Assert.Equal("Question1", question.Question.Sys.Id);
    }

    [Fact]
    public async Task GetQuestionById_Should_ReturnAsNormal_If_PastSubmission_IsComplete()
    {
        _controller.TempData["param"] = "SectionName+SectionId";

        var questionRef = "Question1";

        List<Submission> submissionList = new List<Submission>()
            {
                new Submission()
                {
                    Id = 1,
                    EstablishmentId = 0,
                    Completed = true,
                    SectionId = "SectionId",
                    SectionName = "SectionName",
                    Maturity = null,
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = null,
                    DateCompleted = null
                }
            };

        var query = Arg.Any<IQueryable<Submission>>();
        _databaseSubstitute.FirstOrDefaultAsync(query).Returns(submissionList[0]);

        _controller.TempData[TempDataConstants.Questions] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataQuestions() { QuestionRef = questionRef, AnswerRef = null, SubmissionId = null });

        var result = await _controller.GetQuestionById(null, null, _submitAnswerCommand, CancellationToken.None);
        Assert.IsType<ViewResult>(result);

        var viewResult = result as ViewResult;

        var model = viewResult!.Model;

        Assert.IsType<QuestionViewModel>(model);

        var question = model as QuestionViewModel;

        Assert.NotNull(question);
        Assert.Equal("Question1", question.Question.Sys.Id);
    }

    [Fact]
    public async Task GetQuestionById_Should_ReturnUnansweredQuestion_If_PastSubmission_IsNotCompleted()
    {
        _controller.TempData["param"] = "SectionName+SectionId";

        var questionRef = "Question1";

        List<Submission> submissionList = new List<Submission>()
            {
                new Submission()
                {
                    Id = 1,
                    EstablishmentId = 0,
                    Completed = false,
                    SectionId = "SectionId",
                    SectionName = "SectionName",
                    Maturity = null,
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = null,
                    DateCompleted = null
                }
            };

        List<QuestionWithAnswer> questionWithAnswerList = new List<QuestionWithAnswer>()
            {
                new QuestionWithAnswer()
                {
                    QuestionRef = questionRef,
                    QuestionText = "Question One",
                    AnswerRef = "Answer1",
                    AnswerText = "Question 1 - Answer 1"
                }
            };
        var query = Arg.Any<IQueryable<Domain.Submissions.Models.Submission>>();
        _databaseSubstitute.FirstOrDefaultAsync(query).Returns(submissionList[0]);

        _getLatestResponseListForSubmissionQuerySubstitute.GetResponseListByDateCreated(1).Returns(questionWithAnswerList);

        _controller.TempData[TempDataConstants.Questions] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataQuestions() { QuestionRef = questionRef, AnswerRef = null, SubmissionId = null });

        var result = await _controller.GetQuestionById(null, null, _submitAnswerCommand, CancellationToken.None);
        Assert.IsType<ViewResult>(result);

        var viewResult = result as ViewResult;

        var model = viewResult!.Model;

        Assert.IsType<QuestionViewModel>(model);

        var question = model as QuestionViewModel;

        Assert.NotNull(question);
        Assert.Equal("Question2", question.Question.Sys.Id);
    }

    [Fact]
    public async Task GetQuestionById_Should_RedirectToCheckAnswersController_If_PastSubmission_IsNotCompleted_And_ThereIsNo_NextQuestion()
    {
        _controller.TempData["param"] = "SectionName+SectionId";

        var questionRef = "Question2";

        List<Submission> submissionList = new List<Submission>()
            {
                new Submission()
                {
                    Id = 1,
                    EstablishmentId = 0,
                    Completed = false,
                    SectionId = "SectionId",
                    SectionName = "SectionName",
                    Maturity = null,
                    RecomendationId = 0,
                    DateCreated = DateTime.UtcNow,
                    DateLastUpdated = null,
                    DateCompleted = null
                }
            };

        List<QuestionWithAnswer> questionWithAnswerList = new List<QuestionWithAnswer>()
            {
                new QuestionWithAnswer()
                {
                    QuestionRef = questionRef,
                    QuestionText = "Question Two",
                    AnswerRef = "Answer4",
                    AnswerText = "Question 2 - Answer 4"
                }
            };

        var query = Arg.Any<IQueryable<Domain.Submissions.Models.Submission>>();
        _databaseSubstitute.FirstOrDefaultAsync(query).Returns(submissionList[0]);

        _getLatestResponseListForSubmissionQuerySubstitute.GetResponseListByDateCreated(1).Returns(questionWithAnswerList);

        _controller.TempData[TempDataConstants.Questions] = Newtonsoft.Json.JsonConvert.SerializeObject(new TempDataQuestions() { QuestionRef = questionRef, AnswerRef = null, SubmissionId = null });

        var result = await _controller.GetQuestionById(null, null, _submitAnswerCommand, CancellationToken.None);
        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("CheckAnswers", redirectToActionResult.ControllerName);
        Assert.Equal("CheckAnswersPage", redirectToActionResult.ActionName);
        Assert.NotNull(_controller.TempData[TempDataConstants.CheckAnswers]);
        Assert.IsType<string>(_controller.TempData[TempDataConstants.CheckAnswers]);
        var id = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDataCheckAnswers>(_controller.TempData[TempDataConstants.CheckAnswers] as string ?? "")?.SubmissionId;
        Assert.Equal(1, id);
    }

    [Fact]
    public async Task GetQuestionById_Should_ThrowException_When_IdIsNull()
    {
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionById(null, null, _submitAnswerCommand, CancellationToken.None));
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
            QuestionId = "Question1",
            ChosenAnswerId = "Answer1",
        };

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
        Assert.NotNull(_controller.TempData[TempDataConstants.Questions]);
        Assert.IsType<string>(_controller.TempData[TempDataConstants.Questions]);
        var id = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDataQuestions>(_controller.TempData[TempDataConstants.Questions] as string ?? "")?.QuestionRef;
        Assert.Equal("Question2", id);
    }

    [Fact]
    public async void SubmitAnswer_Should_RedirectTo_CheckAnswers_When_NextQuestionId_IsNull()
    {
        var submitAnswerDto = new SubmitAnswerDto()
        {
            QuestionId = "Question2",
            ChosenAnswerId = "Answer1",
            SubmissionId = 1,
            Params = "SectionName+SectionId"
        };

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("CheckAnswers", redirectToActionResult.ControllerName);
        Assert.Equal("CheckAnswersPage", redirectToActionResult.ActionName);
        Assert.NotNull(_controller.TempData[TempDataConstants.CheckAnswers]);
        Assert.IsType<string>(_controller.TempData[TempDataConstants.CheckAnswers]);
        var id = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDataCheckAnswers>(_controller.TempData[TempDataConstants.CheckAnswers] as string ?? "")?.SubmissionId;
        Assert.Equal(submitAnswerDto.SubmissionId, id);
    }

    //[Fact]
    //public async void SubmitAnswer_Should_RedirectTo_CheckAnswers_When_NextQuestionIsAnswered()
    //{
    //    var submitAnswerDto = new SubmitAnswerDto()
    //    {
    //        QuestionId = "Question1",
    //        ChosenAnswerId = "Answer1",
    //        SubmissionId = 1,
    //        Params = "SectionName+SectionId"
    //    };

    //    Domain.Questions.Models.Question question = new Domain.Questions.Models.Question()
    //    {
    //        Id = 1,
    //        ContentfulRef = "Question2"
    //    };

    //    _databaseSubstitute.GetQuestion(question => question.Id == 1).Returns(question);
    //    _databaseSubstitute.GetResponseList(response => response.SubmissionId == 1).Returns(
    //        new Domain.Responses.Models.Response[]
    //        {
    //                new Domain.Responses.Models.Response()
    //                {
    //                    QuestionId = 1,
    //                    Question = question
    //                }
    //        });

    //    var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

    //    Assert.IsType<RedirectToActionResult>(result);

    //    var redirectToActionResult = result as RedirectToActionResult;

    //    Assert.NotNull(redirectToActionResult);
    //    Assert.Equal("CheckAnswers", redirectToActionResult.ControllerName);
    //    Assert.Equal("CheckAnswersPage", redirectToActionResult.ActionName);
    //    Assert.NotNull(_controller.TempData[TempDataConstants.CheckAnswers]);
    //    Assert.IsType<string>(_controller.TempData[TempDataConstants.CheckAnswers]);
    //    var id = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDataCheckAnswers>(_controller.TempData[TempDataConstants.CheckAnswers] as string ?? "")?.SubmissionId;
    //    Assert.Equal(submitAnswerDto.SubmissionId, id);
    //}

    [Fact]
    public async void SubmitAnswer_Should_RedirectTo_SameQuestion_When_ChosenAnswerId_IsNull()
    {
        var submitAnswerDto = new SubmitAnswerDto()
        {
            QuestionId = "Question1",
            ChosenAnswerId = null!,
        };

        _controller.ModelState.AddModelError("ChosenAnswerId", "Required");

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
        Assert.NotNull(_controller.TempData[TempDataConstants.Questions]);
        Assert.IsType<string>(_controller.TempData[TempDataConstants.Questions]);
        var id = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDataQuestions>(_controller.TempData[TempDataConstants.Questions] as string ?? "")?.QuestionRef;
        Assert.Equal(submitAnswerDto.QuestionId, id);
    }

    [Fact]
    public async void SubmitAnswer_Should_RedirectTo_SameQuestion_When_NextQuestionId_And_ChosenAnswerId_IsNull()
    {
        var submitAnswerDto = new SubmitAnswerDto()
        {
            QuestionId = "Question1",
            ChosenAnswerId = null!
        };

        _controller.ModelState.AddModelError("ChosenAnswerId", "Required");

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
        Assert.NotNull(_controller.TempData[TempDataConstants.Questions]);
        Assert.IsType<string>(_controller.TempData[TempDataConstants.Questions]);
        var id = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDataQuestions>(_controller.TempData[TempDataConstants.Questions] as string ?? "")?.QuestionRef;
        Assert.Equal(submitAnswerDto.QuestionId, id);
    }

    [Fact]
    public async void SubmitAnswer_Params_Should_Parse_When_Params_IsNotNull()
    {
        var submitAnswerDto = new SubmitAnswerDto()
        {
            QuestionId = "Question1",
            ChosenAnswerId = "Answer1",
            Params = "SectionName+SectionId"
        };

        var result = await _controller.SubmitAnswer(submitAnswerDto, _submitAnswerCommand);

        Assert.IsType<RedirectToActionResult>(result);

        var redirectToActionResult = result as RedirectToActionResult;

        Assert.NotNull(redirectToActionResult);
        Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
        Assert.NotNull(_controller.TempData[TempDataConstants.Questions]);
        Assert.IsType<string>(_controller.TempData[TempDataConstants.Questions]);
        var id = Newtonsoft.Json.JsonConvert.DeserializeObject<TempDataQuestions>(_controller.TempData[TempDataConstants.Questions] as string ?? "")?.QuestionRef;
        Assert.Equal("Question2", id);
    }

    private void SetParams(string sectionId)
    {
        _controller.TempData["param"] = CreateParams(sectionId);
    }

    private static string CreateParams(string sectionId) => $"SectionName+{sectionId}";
}
