using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Response.Queries
{
    public class GetLatestResponseListForSubmissionQueryTests
    {
        private readonly Mock<IPlanTechDbContext> _planTechDbContextMock;
        private readonly GetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQuery;

        public GetLatestResponseListForSubmissionQueryTests()
        {
            _planTechDbContextMock = new Mock<IPlanTechDbContext>();

            _getLatestResponseListForSubmissionQuery = new GetLatestResponseListForSubmissionQuery(_planTechDbContextMock.Object);
        }

        [Fact]
        public async Task GetLatestResponseListForSubmissionQuery_Returns_QuestionWithAnswerList()
        {
            List<Domain.Responses.Models.Response> responseList = new List<Domain.Responses.Models.Response>()
            {
                new Domain.Responses.Models.Response()
                {
                    Id = 1,
                    SubmissionId = 1,
                    QuestionId = 1,
                    Question = new Domain.Questions.Models.Question()
                    {
                        Id = 1,
                        QuestionText = "Question Text",
                        ContentfulRef = "QuestionRef-1"
                    },
                    AnswerId = 1,
                    Answer = new Domain.Answers.Models.Answer()
                    {
                        Id = 1,
                        AnswerText = "Answer Text",
                        ContentfulRef = "AnswerRef-1"
                    }
                }
            };

            List<QuestionWithAnswer>? questionWithAnswerList = new List<QuestionWithAnswer>()
            {
                new QuestionWithAnswer()
                {
                    QuestionRef = "QuestionRef-1",
                    QuestionText = "Question Text",
                    AnswerRef = "AnswerRef-1",
                    AnswerText = "Answer Text"
                }
            };

            _planTechDbContextMock.Setup(m => m.GetResponses).Returns(responseList.AsQueryable());
            _planTechDbContextMock.Setup(m => m.ToListAsync(It.IsAny<IQueryable<QuestionWithAnswer>>())).ReturnsAsync(questionWithAnswerList);

            var result = await _getLatestResponseListForSubmissionQuery.GetLatestResponseListForSubmissionBy(1);

            Assert.IsType<List<QuestionWithAnswer>>(result);
            Assert.Equal(questionWithAnswerList, result);
        }
    }
}