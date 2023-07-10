using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Application.Response.Commands;
using Dfe.PlanTech.Application.Response.Interface;
using Dfe.PlanTech.Application.Submission.Commands;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Dfe.PlanTech.Web.Helpers;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.Controllers
{
    public class QuestionsControllerTests
    {
        private readonly List<Question> _questions = new() {
           new Question()
           {
               Sys = new SystemDetails(){
                   Id = "Question1"
               },
               Text = "Question One",
               HelpText = "Explanation",
               Answers = new[] {
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer1" },
                       Text = "Question 1 - Answer 1"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer2" },
                       Text = "Question 1 - Answer 2"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer3" },
                       Text = "Question 1 - Answer 3"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer4" },
                       Text = "Question 1 - Answer 4"
                   }
               }
           },
           new Question()
           {
               Sys = new SystemDetails(){
                   Id = "Question2"
               },
               Text = "Question Two",
               HelpText = "Explanation",
               Answers = new[] {
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer1" },
                       Text = "Question 2 - Answer 1"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer2" },
                       Text = "Question 2 - Answer 2"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer3" },
                       Text = "Question 2 - Answer 3"
                   },
                   new Answer(){
                       Sys = new SystemDetails() { Id = "Answer4" },
                       Text = "Question 2 - Answer 4"
                   }
               }
           }

       };

        private readonly QuestionsController _controller;
        private readonly Mock<IQuestionnaireCacher> _questionnaireCacherMock;

        private readonly ICacher _cacher;

        public QuestionsControllerTests()
        {
            Mock<IContentRepository> repositoryMock = MockRepository();
            _questionnaireCacherMock = MockQuestionnaireCacher();

            var mockLogger = new Mock<ILogger<QuestionsController>>();
            var historyMock = new Mock<IUrlHistory>();
            var databaseMock = new Mock<IPlanTechDbContext>();

            GetQuestionQuery query = new GetQuestionQuery(_questionnaireCacherMock.Object, repositoryMock.Object);

            ICreateQuestionCommand createQuestionCommand = new CreateQuestionCommand(databaseMock.Object);
            IRecordQuestionCommand recordQuestionCommand = new RecordQuestionCommand(databaseMock.Object, createQuestionCommand);

            ICreateAnswerCommand createAnswerCommand = new CreateAnswerCommand(databaseMock.Object);
            IRecordAnswerCommand recordAnswerCommand = new RecordAnswerCommand(databaseMock.Object, createAnswerCommand);
            ICreateResponseCommand createResponseCommand = new CreateResponseCommand(databaseMock.Object);
            ICreateSubmissionCommand createSubmissionCommand = new CreateSubmissionCommand(databaseMock.Object);

            _controller = new QuestionsController(mockLogger.Object, historyMock.Object, query, recordQuestionCommand, recordAnswerCommand, createSubmissionCommand, createResponseCommand, null);

            _cacher = new Cacher(new CacheOptions(), new MemoryCache(new MemoryCacheOptions()));
        }

        private static Mock<IQuestionnaireCacher> MockQuestionnaireCacher()
        {
            var mock = new Mock<IQuestionnaireCacher>();
            mock.Setup(questionnaireCache => questionnaireCache.Cached).Returns(new QuestionnaireCache()).Verifiable();
            mock.Setup(questionnaireCache => questionnaireCache.SaveCache(It.IsAny<QuestionnaireCache>())).Verifiable();

            return mock;
        }

        private Mock<IContentRepository> MockRepository()
        {
            var repositoryMock = new Mock<IContentRepository>();
            repositoryMock.Setup(repo => repo.GetEntities<Question>(It.IsAny<IGetEntitiesOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync((IGetEntitiesOptions options, CancellationToken _) =>
            {
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

            repositoryMock.Setup(repo => repo.GetEntityById<Question>(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((string id, int include, CancellationToken _) => _questions.FirstOrDefault(question => question.Sys.Id == id));
            return repositoryMock;
        }

        [Fact]
        public async Task GetQuestionById_Should_ReturnQuestionPage_When_FetchingQuestionWithValidId()
        {
            var id = "Question1";

            var result = await _controller.GetQuestionById(id, null, CancellationToken.None);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<QuestionViewModel>(model);

            var question = model as QuestionViewModel;

            Assert.NotNull(question);
            Assert.Equal("Question One", question.Question.Text);
        }

        [Fact]
        public async Task GetQuestionById_Should_ThrowException_When_IdIsNull()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionById(null!, null, CancellationToken.None));
        }

        [Fact]
        public async Task GetQuestionById_Should_ThrowException_When_IdIsNotFound()
        {
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _controller.GetQuestionById("not a real question id", null, CancellationToken.None));
        }

        [Fact]
        public async Task GetQuestionById_Should_SaveSectionTitle_When_NotNull()
        {
            var id = "Question1";
            var sectionTitle = "Section title";

            await _controller.GetQuestionById(id, sectionTitle, CancellationToken.None);

            _questionnaireCacherMock.Verify();
        }

        [Fact]
        public async Task GetQuestionById_Should_NotSaveSectionTitle_When_Null()
        {
            var id = "Question1";
            var result = await _controller.GetQuestionById(id, null, CancellationToken.None);

            _questionnaireCacherMock.Verify(cache => cache.Cached, Times.Never);
            _questionnaireCacherMock.Verify(cache => cache.SaveCache(It.IsAny<QuestionnaireCache>()), Times.Never);
        }

        [Fact]
        public void SubmitAnswer_Should_ThrowException_When_NullArgument()
        {
            Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.SubmitAnswer(null!));
        }

        [Fact]
        public async void SubmitAnswer_Should_RedirectToNextQuestion_When_NextQuestionId_Exists()
        {
            var submitAnswerDto = new SubmitAnswerDto()
            {
                QuestionId = "Question1",
                ChosenAnswerId = "Answer1",
                NextQuestionId = "Question2"
            };

            var result = await _controller.SubmitAnswer(submitAnswerDto);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
            Assert.Equal(submitAnswerDto.NextQuestionId, id.Value);
        }

        [Fact]
        public async void SubmitAnswer_Should_RedirectTo_CheckYourAnswers_When_NextQuestionId_IsNull()
        {
            var submitAnswerDto = new SubmitAnswerDto()
            {
                QuestionId = "Question1",
                ChosenAnswerId = "Answer1"
            };

            var result = await _controller.SubmitAnswer(submitAnswerDto);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("Pages", redirectToActionResult.ControllerName);
            Assert.Equal("GetByRoute", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            Assert.Equal("check-answers", redirectToActionResult.RouteValues["route"]);
        }

        [Fact]
        public async void SubmitAnswer_Should_RedirectTo_SameQuestion_When_ChosenAnswerId_IsNull()
        {
            var submitAnswerDto = new SubmitAnswerDto()
            {
                QuestionId = "Question1",
                ChosenAnswerId = null!,
                NextQuestionId = "Question2"
            };

            _controller.ModelState.AddModelError("ChosenAnswerId", "Required");

            var result = await _controller.SubmitAnswer(submitAnswerDto);

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
                QuestionId = "Question1",
                ChosenAnswerId = null!
            };

            _controller.ModelState.AddModelError("ChosenAnswerId", "Required");

            var result = await _controller.SubmitAnswer(submitAnswerDto);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("GetQuestionById", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            var id = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "id");
            Assert.Equal(submitAnswerDto.QuestionId, id.Value);
        }
    }
}