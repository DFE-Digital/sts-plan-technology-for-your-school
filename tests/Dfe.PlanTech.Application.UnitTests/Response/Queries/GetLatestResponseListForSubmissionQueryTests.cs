using Bogus;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Responses.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Domain.Responses.Models;
using NSubstitute;
using Answer = Dfe.PlanTech.Domain.Answers.Models.Answer;
using Question = Dfe.PlanTech.Domain.Questions.Models.Question;

namespace Dfe.PlanTech.Application.UnitTests.Responses.Queries;
public class GetLatestResponseListForSubmissionQueryTests
{
    private const int ESTABLISHMENT_ID = 1;
    private const int USER_ID = 1;
    private const int SECTION_COUNT = 3;
    private const int QUESTION_PER_SECTION_COUNT = 5;
    private const int ANSWER_PER_QUESTION_COUNT = 4;

    private IPlanTechDbContext _planTechDbContextSubstitute;
    private readonly GetLatestResponseListForSubmissionQuery _getLatestResponseListForSubmissionQuery;

    private readonly List<Domain.Submissions.Models.Submission> _submissions;


    private readonly List<Section> _sections;

    private int responseId = 1;
    private int questionId = 1;
    private int answerId = 1;

    private readonly string[] maturities = new[] { "Low", "Medium", "High" };

    public GetLatestResponseListForSubmissionQueryTests()
    {
        var sysFaker = new Faker<SystemDetails>().RuleFor(sys => sys.Id, faker => faker.Random.AlphaNumeric(20));
        var generatedSystemDetails = sysFaker.GenerateForever();


        var answerFaker = new Faker<Domain.Questionnaire.Models.Answer>()
                            .RuleFor(answer => answer.Sys, generatedSystemDetails.First())
                            .RuleFor(answer => answer.Text, faker => faker.Lorem.Sentence(faker.Random.Int(1, 5)));

        var questionFaker = new Faker<Domain.Questionnaire.Models.Question>()
                            .RuleFor(question => question.Sys, generatedSystemDetails.First())
                            .RuleFor(question => question.Text, faker => faker.Lorem.Sentence())
                            .RuleFor(question => question.Answers, _ => answerFaker.Generate(ANSWER_PER_QUESTION_COUNT).ToArray());

        var sectionFaker = new Faker<Section>()
                                .RuleFor(section => section.Sys, generatedSystemDetails.First())
                                .RuleFor(section => section.Questions, questionFaker.Generate(QUESTION_PER_SECTION_COUNT).ToArray());


        _sections = sectionFaker.Generate(SECTION_COUNT);

        var sectionIds = _sections.Select(section => section.Sys.Id).ToArray();

        int submissionId = 0;

        var submissionFaker = new Faker<Domain.Submissions.Models.Submission>()
                                    .RuleFor(submission => submission.Completed, faker => faker.Random.Bool())
                                    .RuleFor(submission => submission.DateCreated, faker => faker.Date.Past())
                                    .RuleFor(submission => submission.DateCompleted, (faker, submission) => submission.Completed ? faker.Date.Future(1, submission.DateCreated) : null)
                                    .RuleFor(submission => submission.EstablishmentId, ESTABLISHMENT_ID)
                                    .RuleFor(submission => submission.Id, faker =>
                                    {
                                        submissionId++;
                                        return submissionId;
                                    })
                                    .RuleFor(submission => submission.Maturity, (faker, submission) => submission.Completed ? faker.PickRandom(maturities) : null)
                                    .RuleFor(submission => submission.SectionId, faker => faker.PickRandom(sectionIds))
                                    .RuleFor(submission => submission.SectionName, (faker, submission) => $"Section {submission.Id}")
                                    .RuleFor(submission => submission.Responses, (faker, submission) =>
                                    {
                                        var response = new Response()
                                        {
                                            response.DateCreated = faker.Date.Between(submission.DateCreated, submission.DateCompleted ?? DateTime.UtcNow),
                                        }
                                    });

        _submissions = submissionFaker.Generate(20);

        _planTechDbContextSubstitute = Substitute.For<IPlanTechDbContext>();
        _getLatestResponseListForSubmissionQuery = new GetLatestResponseListForSubmissionQuery(_planTechDbContextSubstitute);

        _planTechDbContextSubstitute.GetSubmissions.Returns(_submissions.AsQueryable());
    }

    [Fact]
    public void Test()
    {
        Console.WriteLine("");
    }

    /*
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

                List<Response> responseList = new()
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
            */

    private IEnumerable<Response> GenerateResponses(Domain.Submissions.Models.Submission submission, Faker faker)
    {
        var section = _sections.First(section => section.Sys.Id == submission.SectionId);

        int timesAnswered = faker.Random.Int(1, 5);

        while (timesAnswered > 0)
        {
            var responseCount = submission.Completed ? section.Questions.Length : faker.Random.Int(1, section.Questions.Length);

            for (var x = 0; x < responseCount; x++)
            {
                Response response = GenerateResponse(submission, faker, section, x);

                yield return response;
            }
        }
    }

    private Response GenerateResponse(Domain.Submissions.Models.Submission submission, Faker faker, Section section, int x)
    {
        var question = section.Questions[x];
        var answer = faker.PickRandom(question.Answers);

        var response = new Response()
        {
            DateCreated = faker.Date.Between(submission.DateCreated, submission.DateCompleted ?? DateTime.UtcNow),
            Id = responseId++,
            UserId = USER_ID,
            SubmissionId = submission.Id,
            Submission = submission,
            Question = new Question()
            {
                QuestionText = question.Text,
                ContentfulRef = question.Sys.Id,
                Id = questionId++,
            },
            Answer = new Answer()
            {
                AnswerText = answer.Text,
                ContentfulRef = answer.Sys.Id,
                Id = answerId++,
            },
            Maturity = faker.PickRandom(maturities),
        };
        return response;
    }

    private IEnumerable<Response>
}