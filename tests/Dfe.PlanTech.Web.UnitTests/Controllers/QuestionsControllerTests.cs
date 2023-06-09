using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Questionnaire.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Application.Models;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
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
            _controller = new QuestionsController(mockLogger.Object);
            _query = new GetQuestionQuery(repositoryMock.Object);
        }


        [Fact]
        public async Task Should_ReturnQuestionPage_When_FetchingQuestionById()
        {
            var id = "Question1";

            var result = await _controller.GetQuestionById(id, _query);
            Assert.IsType<ViewResult>(result);

            var viewResult = result as ViewResult;

            var model = viewResult!.Model;

            Assert.IsType<Question>(model);

            var question = model as Question;

            Assert.NotNull(question);
            Assert.Equal("Question One", question.Text);
        }
        
        [Fact]
        public async Task Should_ThrowException_When_IdIsNull()
        {
            await Assert.ThrowsAnyAsync<ArgumentNullException>(() => _controller.GetQuestionById(null!, _query));
        }
    }
}