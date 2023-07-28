using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Submission.Queries
{
    public class GetQuestionQueryTests
    {
        private readonly Mock<IPlanTechDbContext> _planTechDbContextMock;
        private readonly GetQuestionQuery _getQuestionQuery;

        public GetQuestionQueryTests()
        {
            _planTechDbContextMock = new Mock<IPlanTechDbContext>();

            _getQuestionQuery = new GetQuestionQuery(_planTechDbContextMock.Object);
        }

        [Fact]
        public async Task GetQuestionQuery_Returns_Question()
        {
            _planTechDbContextMock.Setup(m => m.GetQuestion(question => question.Id == 1)).ReturnsAsync(
                new Domain.Questions.Models.Question()
                {
                    Id = 1,
                    QuestionText = "Question Text",
                    ContentfulRef = "QuestionRef-1"
                });

            var result = await _getQuestionQuery.GetQuestionBy(1);

            Assert.IsType<Domain.Questions.Models.Question>(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Question Text", result.QuestionText);
            Assert.Equal("QuestionRef-1", result.ContentfulRef);
        }
    }
}