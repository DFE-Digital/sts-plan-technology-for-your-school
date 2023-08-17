using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Domain.Answers.Models;
using NSubstitute;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.UnitTests.Submission.Queries
{
    public class GetAnswerQueryTests
    {
        private IPlanTechDbContext _planTechDbContextMock;
        private readonly GetAnswerQuery _getAnswerQuery;

        public GetAnswerQueryTests()
        {
            _planTechDbContextMock = Substitute.For<IPlanTechDbContext>();

            _getAnswerQuery = new GetAnswerQuery(_planTechDbContextMock);
        }

        [Fact]
        public async Task GetAnswerQuery_Returns_Answer()
        {
            var output = new Domain.Answers.Models.Answer()
            {
                Id = 1,
                AnswerText = "Answer Text",
                ContentfulRef = "AnswerRef-1"
            };
            _planTechDbContextMock.GetAnswer(Arg.Any<Expression<Func<Answer, bool>>>()).Returns(Task.FromResult(output));

            var result = await _getAnswerQuery.GetAnswerBy(1);

            Assert.IsType<Domain.Answers.Models.Answer>(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Answer Text", result.AnswerText);
            Assert.Equal("AnswerRef-1", result.ContentfulRef);
        }
    }
}