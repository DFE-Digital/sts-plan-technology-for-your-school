using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Responses.Queries;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using NSubstitute;
using Answer = Dfe.PlanTech.Domain.Answers.Models.Answer;
using Question = Dfe.PlanTech.Domain.Questions.Models.Question;

namespace Dfe.PlanTech.Application.UnitTests.Responses.Queries
{
    public class GetLatestResponseListForSubmissionQueryTests
    {
        private IPlanTechDbContext _planTechDbContextSubstitute;
        private readonly GetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQuery;

        public GetLatestResponseListForSubmissionQueryTests()
        {
            _planTechDbContextSubstitute = Substitute.For<IPlanTechDbContext>();

            _getLatestResponseListForSubmissionQuery = new GetLatestResponseListForSubmissionQuery(_planTechDbContextSubstitute);
        }

        [Fact]
        public async Task GetLatestResponseListForSubmissionBy_Returns_QuestionWithAnswerList()
        {
            List<Response> responseList = new()
            {
                new()
                {
                    Id = 1,
                    SubmissionId = 1,
                    QuestionId = 1,
                    Question = new Question()
                    {
                        Id = 1,
                        QuestionText = "Question Text",
                        ContentfulRef = "QuestionRef-1"
                    },
                    AnswerId = 1,
                    Answer = new Answer()
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

            _planTechDbContextSubstitute.GetResponses.Returns(responseList.AsQueryable());
            _planTechDbContextSubstitute.ToListAsync(Arg.Any<IQueryable<QuestionWithAnswer>>()).Returns(Task.FromResult(questionWithAnswerList));

            var result = await _getLatestResponseListForSubmissionQuery.GetLatestResponseListForSubmissionBy(1);

            Assert.IsType<List<QuestionWithAnswer>>(result);
            Assert.Equal(questionWithAnswerList, result);
        }

        [Fact]
        public async Task GetResponseListByDateCreated_Returns_QuestionWithAnswerList_In_DateCreated_DescendingOrder()
        {
            DateTime responseOneDateCreated = new DateTime(2000, 01, 01, 04, 08, 16);
            DateTime responseTwoDateCreated = new DateTime(2000, 01, 01, 08, 16, 32);

            List<Response> responseList = new ()
            {
                new Response()
                {
                    Id = 1,
                    SubmissionId = 1,
                    QuestionId = 1,
                    Question = new Question
                    {
                        Id = 1,
                        QuestionText = "Question Text",
                        ContentfulRef = "QuestionRef-1"
                    },
                    AnswerId = 1,
                    Answer = new Answer()
                    {
                        Id = 1,
                        AnswerText = "Answer Text",
                        ContentfulRef = "AnswerRef-1"
                    },
                    DateCreated = responseOneDateCreated
                },

                new Response()
                {
                    Id = 2,
                    SubmissionId = 1,
                    QuestionId = 2,
                    Question = new Question()
                    {
                        Id = 2,
                        QuestionText = "Question Text",
                        ContentfulRef = "QuestionRef-2"
                    },
                    AnswerId = 2,
                    Answer = new Answer()
                    {
                        Id = 2,
                        AnswerText = "Answer Text",
                        ContentfulRef = "AnswerRef-2"
                    },
                    DateCreated = responseTwoDateCreated
                }
            };

            List<QuestionWithAnswer>? questionWithAnswerList = new()
            {
                new QuestionWithAnswer()
                {
                    QuestionRef = "QuestionRef-2",
                    QuestionText = "Question Text",
                    AnswerRef = "AnswerRef-2",
                    AnswerText = "AnswerText",
                    DateCreated = responseTwoDateCreated
                },

                new QuestionWithAnswer()
                {
                    QuestionRef = "QuestionRef-1",
                    QuestionText = "Question Text",
                    AnswerRef = "AnswerRef-1",
                    AnswerText = "Answer Text",
                    DateCreated = responseOneDateCreated
                }
            };

            _planTechDbContextSubstitute.GetResponses.Returns(responseList.AsQueryable());
            _planTechDbContextSubstitute.ToListAsync(Arg.Any<IQueryable<QuestionWithAnswer>>()).Returns(Task.FromResult(questionWithAnswerList));

            var result = await _getLatestResponseListForSubmissionQuery.GetResponseListByDateCreated(1);

            Assert.IsType<List<QuestionWithAnswer>>(result);
            Assert.Equal(questionWithAnswerList, result);
        }
    }
}