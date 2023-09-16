using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Queries;
using Dfe.PlanTech.Domain.Answers.Models;
using NSubstitute;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.UnitTests.Submission.Queries
{
    public class GetAnswerQueryTests
    {
        private IPlanTechDbContext _planTechDbContextSubstitute;
        private readonly GetAnswerQuery _getAnswerQuery;

        public GetAnswerQueryTests()
        {
            _planTechDbContextSubstitute = Substitute.For<IPlanTechDbContext>();

            _getAnswerQuery = new GetAnswerQuery(_planTechDbContextSubstitute);
        }

        [Fact]
        public async Task GetAnswerQuery_Returns_Answer()
        {
            var output = new Answer()
            {
                Id = 1,
                AnswerText = "Answer Text",
                ContentfulRef = "AnswerRef-1"
            };
            _planTechDbContextSubstitute.GetAnswer(Arg.Any<Expression<Func<Answer, bool>>>()).Returns(output);

            var result = await _getAnswerQuery.GetAnswerBy(1);

            Assert.IsType<Answer>(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Answer Text", result.AnswerText);
            Assert.Equal("AnswerRef-1", result.ContentfulRef);
        }
    }
}