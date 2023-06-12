using Contentful.Core.Models;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
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
                        Text = "Question 1 - Answer 1"
                    },
                    new Answer(){
                        Text = "Question 1 - Answer 2"
                    },
                    new Answer(){
                        Text = "Question 1 - Answer 3"
                    },
                    new Answer(){
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
                        Text = "Question 2 - Answer 1"
                    },
                    new Answer(){
                        Text = "Question 2 - Answer 2"
                    },
                    new Answer(){
                        Text = "Question 2 - Answer 3"
                    },
                    new Answer(){
                        Text = "Question 2 - Answer 4"
                    }
                }
            }

        };

        private readonly QuestionsController _controller;
        private readonly GetQuestionQuery _query;

        private readonly ICacher _cacher;
        
        public QuestionsControllerTests()
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

            var mockLogger = new Mock<ILogger<QuestionsController>>();
            
            var mockCacher = new Mock<ICacher>();
            
            _controller = new QuestionsController(mockCacher.Object, mockLogger.Object);
            _query = new GetQuestionQuery(repositoryMock.Object);
            
            _cacher = new Cacher(new CacheOptions(), new MemoryCache(new MemoryCacheOptions()));
        }


        [Fact]
        public async Task GetQuestionById_Should_ReturnQuestionPage_When_FetchingQuestionWithValidId()
        {
            var id = "Question1";

            var result = await _controller.GetQuestionById(id, _query);
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
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionById(null!, _query));
        }

        [Fact]
        public async Task GetQuestionById_Should_ThrowException_When_IdIsNotFound()
        {
            await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _controller.GetQuestionById("not a real question id", _query));
        }

        [Fact]
        public async Task SubmitAnswer_Should_ThrowException_When_NullArgument()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.SubmitAnswer(null!));
        }

        [Fact]
        public async Task SubmitAnswer_Should_RedirectToNextQuestion_When_NextQuestionId_Exists()
        {
            var submitAnswerDto = new SubmitAnswerDto(){
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
        public async Task SubmitAnswer_Should_RedirectToSelfAssessment_When_NextQuestionId_IsNull()
        {
            var submitAnswerDto = new SubmitAnswerDto();

            var result = await _controller.SubmitAnswer(submitAnswerDto);

            Assert.IsType<RedirectToActionResult>(result);

            var redirectToActionResult = result as RedirectToActionResult;

            Assert.NotNull(redirectToActionResult);
            Assert.Equal("Pages", redirectToActionResult.ControllerName);
            Assert.Equal("GetByRoute", redirectToActionResult.ActionName);
            Assert.NotNull(redirectToActionResult.RouteValues);
            
            var route = redirectToActionResult.RouteValues.FirstOrDefault(routeValue => routeValue.Key == "route");
            Assert.Equal("self-assessment", route.Value);
        }
    }
}