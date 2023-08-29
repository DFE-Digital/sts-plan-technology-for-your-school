using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Queries;
using Dfe.PlanTech.Domain.Questions.Models;
using NSubstitute;
using System.Linq.Expressions;

namespace Dfe.PlanTech.Application.UnitTests.Submission.Queries
{
    public class GetQuestionQueryTests
    {
        private IPlanTechDbContext _planTechDbContextSubstitute;
        private readonly GetQuestionQuery _getQuestionQuery;

        public GetQuestionQueryTests()
        {
            _planTechDbContextSubstitute = Substitute.For<IPlanTechDbContext>();

            _getQuestionQuery = new GetQuestionQuery(_planTechDbContextSubstitute);
        }

        [Fact]
        public async Task GetQuestionQuery_Returns_Question()
        {
            _planTechDbContextSubstitute.GetQuestion(Arg.Any<Expression<Func<Question, bool>>>()).Returns(
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