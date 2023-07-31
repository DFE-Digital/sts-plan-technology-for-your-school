using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Moq;

namespace Dfe.PlanTech.Application.UnitTests.Submission.Queries
{
    public class GetAnswerQueryTests
    {
        private readonly Mock<IPlanTechDbContext> _planTechDbContextMock;
        private readonly GetAnswerQuery _getAnswerQuery;

        public GetAnswerQueryTests()
        {
            _planTechDbContextMock = new Mock<IPlanTechDbContext>();

            _getAnswerQuery = new GetAnswerQuery(_planTechDbContextMock.Object);
        }

        [Fact]
        public async Task GetAnswerQuery_Returns_Answer()
        {
            _planTechDbContextMock.Setup(m => m.GetAnswer(answer => answer.Id == 1)).ReturnsAsync(
                new Domain.Answers.Models.Answer()
                {
                    Id = 1,
                    AnswerText = "Answer Text",
                    ContentfulRef = "AnswerRef-1"
                });

            var result = await _getAnswerQuery.GetAnswerBy(1);

            Assert.IsType<Domain.Answers.Models.Answer>(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Answer Text", result.AnswerText);
            Assert.Equal("AnswerRef-1", result.ContentfulRef);
        }
    }
}