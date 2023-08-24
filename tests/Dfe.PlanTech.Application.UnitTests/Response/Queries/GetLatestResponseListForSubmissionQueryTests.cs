using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Response.Queries;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Response.Queries
{
    public class GetLatestResponseListForSubmissionQueryTests
    {
        private IPlanTechDbContext _planTechDbContextMock;
        private readonly GetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQuery;

        public GetLatestResponseListForSubmissionQueryTests()
        {
            _planTechDbContextMock = Substitute.For<IPlanTechDbContext>();

            _getLatestResponseListForSubmissionQuery = new GetLatestResponseListForSubmissionQuery(_planTechDbContextMock);
        }

        [Fact]
        public async Task GetLatestResponseListForSubmissionBy_Returns_QuestionWithAnswerList()
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

            _planTechDbContextMock.GetResponses.Returns(responseList.AsQueryable());
            _planTechDbContextMock.ToListAsync(Arg.Any<IQueryable<QuestionWithAnswer>>()).Returns(Task.FromResult(questionWithAnswerList));

            var result = await _getLatestResponseListForSubmissionQuery.GetLatestResponseListForSubmissionBy(1);

            Assert.IsType<List<QuestionWithAnswer>>(result);
            Assert.Equal(questionWithAnswerList, result);
        }

        [Fact]
        public async Task GetResponseListByDateCreated_Returns_QuestionWithAnswerList_In_DateCreated_DescendingOrder()
        {
            DateTime responseOneDateCreated = new DateTime(2000, 01, 01, 04, 08, 16);
            DateTime responseTwoDateCreated = new DateTime(2000, 01, 01, 08, 16, 32);

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
                    },
                    DateCreated = responseOneDateCreated
                },

                new Domain.Responses.Models.Response()
                {
                    Id = 2,
                    SubmissionId = 1,
                    QuestionId = 2,
                    Question = new Domain.Questions.Models.Question()
                    {
                        Id = 2,
                        QuestionText = "Question Text",
                        ContentfulRef = "QuestionRef-2"
                    },
                    AnswerId = 2,
                    Answer = new Domain.Answers.Models.Answer()
                    {
                        Id = 2,
                        AnswerText = "Answer Text",
                        ContentfulRef = "AnswerRef-2"
                    },
                    DateCreated = responseTwoDateCreated
                }
            };

            List<QuestionWithAnswer>? questionWithAnswerList = new List<QuestionWithAnswer>()
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

            _planTechDbContextMock.GetResponses.Returns(responseList.AsQueryable());
            _planTechDbContextMock.ToListAsync(Arg.Any<IQueryable<QuestionWithAnswer>>()).Returns(Task.FromResult(questionWithAnswerList));

            var result = await _getLatestResponseListForSubmissionQuery.GetResponseListByDateCreated(1);

            Assert.IsType<List<QuestionWithAnswer>>(result);
            Assert.Equal(questionWithAnswerList, result);
        }
    }
}