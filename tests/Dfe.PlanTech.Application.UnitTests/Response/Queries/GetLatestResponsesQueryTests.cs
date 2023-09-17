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

public class GetLatestResponsesQueryTests
{
    private const int ESTABLISHMENT_ID = 1;
    private const int USER_ID = 1;
    private const int SECTION_COUNT = 4;
    private const int QUESTION_PER_SECTION_COUNT = 5;
    private const int ANSWER_PER_QUESTION_COUNT = 4;

    private IPlanTechDbContext _planTechDbContextSubstitute;
    private readonly GetLatestResponsesQuery _getLatestResponseListForSubmissionQuery;

    private readonly List<Domain.Submissions.Models.Submission> _submissions;


    private readonly List<Section> _sections;

    private int responseId = 1;
    private int questionId = 1;
    private int answerId = 1;

    private readonly string[] maturities = new[] { "Low", "Medium", "High" };

    public GetLatestResponsesQueryTests()
    {
        var faker = new Faker();

        var generatedIds = new HashSet<string>();

        var generateSystemDetails = () =>
        {
            bool createdUniqueId = false;
            string id = "";

            while (!createdUniqueId)
            {
                id = faker.Random.AlphaNumeric(30);
                createdUniqueId = !generatedIds.Contains(id);
            }

            generatedIds.Add(id);

            return new SystemDetails()
            {
                Id = id
            };
        };

        var answerFaker = new Faker<Domain.Questionnaire.Models.Answer>()
                            .RuleFor(answer => answer.Sys, generateSystemDetails)
                            .RuleFor(answer => answer.Text, faker => faker.Lorem.Sentence(faker.Random.Int(1, 5)));

        var questionFaker = new Faker<Domain.Questionnaire.Models.Question>()
                            .RuleFor(question => question.Sys, generateSystemDetails)
                            .RuleFor(question => question.Text, faker => faker.Lorem.Sentence())
                            .RuleFor(question => question.Answers, _ => answerFaker.Generate(ANSWER_PER_QUESTION_COUNT).ToArray());

        var sectionFaker = new Faker<Section>()
                                .RuleFor(section => section.Sys, generateSystemDetails)
                                .RuleFor(section => section.Questions, _ => questionFaker.Generate(QUESTION_PER_SECTION_COUNT).ToArray());


        _sections = sectionFaker.Generate(SECTION_COUNT);

        var sectionIds = _sections.Select(section => section.Sys.Id).ToArray();

        int submissionId = 1;

        var submissionFaker = new Faker<Domain.Submissions.Models.Submission>()
                                    .RuleFor(submission => submission.Completed, faker => faker.Random.Bool())
                                    .RuleFor(submission => submission.DateCreated, faker => faker.Date.Past())
                                    .RuleFor(submission => submission.EstablishmentId, ESTABLISHMENT_ID)
                                    .RuleFor(submission => submission.Id, faker => submissionId++);

        _submissions = submissionFaker.Generate(20);

        //First 2 sections == incomplete, last 2 sections == complete
        for (var x = 0; x < _submissions.Count; x++)
        {
            var submission = _submissions[x];
            var section = x < 2 ? _sections[x] : _sections[faker.Random.Int(2, 3)];
            submission.SectionId = section.Sys.Id;
            submission.SectionName = section.Name;
            submission.Responses = GenerateResponses(submission, faker).ToList();

            if (x < 2) continue;
            submission.Completed = true;
            submission.DateCompleted = faker.Date.Future(1, submission.DateCreated);
            submission.Maturity = faker.PickRandom(maturities);
        }

        _planTechDbContextSubstitute = Substitute.For<IPlanTechDbContext>();
        _planTechDbContextSubstitute.GetSubmissions.Returns(_submissions.AsQueryable());
        _planTechDbContextSubstitute.FirstOrDefaultAsync(Arg.Any<IQueryable<Domain.Submissions.Models.Submission>>(), Arg.Any<CancellationToken>())
                                    .Returns((callInfo) =>
                                    {
                                        var queryable = callInfo.ArgAt<IQueryable<Domain.Submissions.Models.Submission>>(0);

                                        return Task.FromResult(queryable.FirstOrDefault());
                                    });

        _planTechDbContextSubstitute.FirstOrDefaultAsync(Arg.Any<IQueryable<QuestionWithAnswer>>(), Arg.Any<CancellationToken>())
                                    .Returns((callInfo) =>
                                    {
                                        var queryable = callInfo.ArgAt<IQueryable<QuestionWithAnswer>>(0);

                                        return Task.FromResult(queryable.FirstOrDefault());
                                    });

        _planTechDbContextSubstitute.FirstOrDefaultAsync(Arg.Any<IQueryable<SubmissionWithResponses>>(), Arg.Any<CancellationToken>())
        .Returns((callInfo) =>
        {
            var queryable = callInfo.ArgAt<IQueryable<SubmissionWithResponses>>(0);

            return Task.FromResult(queryable.FirstOrDefault());
        });


        _getLatestResponseListForSubmissionQuery = new GetLatestResponsesQuery(_planTechDbContextSubstitute);

    }

    [Fact]
    public async Task GetLatestResponseForQuestion_Should_Return_Latest_Response_For_QuestionId_In_Incomplete_Submission()
    {
        var responsesForIncompleteSubmissionsGroupedByQuestion = GetIncompleteSubmissionForIncompleteSection()
                                                                        .Responses
                                                                        .GroupBy(r => r.Question.ContentfulRef)
                                                                        .ToArray();

        var sectionQuestionsWithResponses = _sections.SelectMany(section => section.Questions)
                                                    .ToDictionary(question => question,
                                                                  question => responsesForIncompleteSubmissionsGroupedByQuestion
                                                                                        .FirstOrDefault(g => g.Key == question.Sys.Id)?
                                                                                        .Select(r => r).ToArray() ?? Array.Empty<Response>());

        var mostAnsweredQuestion = sectionQuestionsWithResponses.OrderByDescending(q => q.Value != null ? q.Value.Length : 0).First();

        var expectedMostRecentResponse = mostAnsweredQuestion.Value!.OrderByDescending(response => response.DateCreated).First();

        var latestResponse = await _getLatestResponseListForSubmissionQuery.GetLatestResponseForQuestion(ESTABLISHMENT_ID, expectedMostRecentResponse.Submission.SectionId, mostAnsweredQuestion.Key.Sys.Id);

        Assert.NotNull(latestResponse);

        Assert.Equal(expectedMostRecentResponse.Question.ContentfulRef, latestResponse.QuestionRef);
        Assert.Equal(expectedMostRecentResponse.Question.QuestionText, latestResponse.QuestionText);

        Assert.Equal(expectedMostRecentResponse.Answer.ContentfulRef, latestResponse.AnswerRef);
        Assert.Equal(expectedMostRecentResponse.Answer.AnswerText, latestResponse.AnswerText);
    }

    [Fact]
    public async Task GetLatestResponseForQuestion_Should_Return_Null_For_Complete_Submission()
    {
        var completedSubmission = GetCompletedSubmissionForCompletedSection();

        var questionIdInCompletedSubmission = completedSubmission.Responses.Select(response => response.Question.ContentfulRef)
                                                                            .First();

        var latestResponse = await _getLatestResponseListForSubmissionQuery.GetLatestResponseForQuestion(ESTABLISHMENT_ID, completedSubmission.SectionId, questionIdInCompletedSubmission);

        Assert.Null(latestResponse);
    }

    [Fact]
    public async Task GetLatestResponses_Should_Return_LatestResponses_For_Incomplete_Submission()
    {
        var incompleteSubmission = GetIncompleteSubmissionForIncompleteSection();

        var responsesForIncompleteSubmissionsGroupedByQuestion = _submissions.Where(submission => !submission.Completed)
                                                                            .SelectMany(submission => submission.Responses)
                                                                            .GroupBy(response => response.Question.ContentfulRef)
                                                                            .Select(responses => responses.OrderByDescending(response => response.DateCreated).First())
                                                                            .ToArray();

        var latestResponse = await _getLatestResponseListForSubmissionQuery.GetLatestResponses(ESTABLISHMENT_ID, incompleteSubmission.SectionId);

        Assert.NotNull(latestResponse);
        Assert.NotNull(latestResponse.Value.Responses);
        Assert.True(latestResponse.Value.Responses.Count > 0);
        Assert.Equal(responsesForIncompleteSubmissionsGroupedByQuestion.Length, latestResponse.Value.Responses.Count);

        foreach (var response in latestResponse.Value.Responses)
        {
            var matching = responsesForIncompleteSubmissionsGroupedByQuestion.FirstOrDefault(r => r.Question.ContentfulRef == response.QuestionRef &&
                                                                                                  r.Answer.ContentfulRef == response.AnswerRef);

            Assert.NotNull(response);
        }
    }

    [Fact]
    public async Task GetLatestResponses_Should_Return_Null_For_Completed_Submission()
    {
        var completeSubmission = GetCompletedSubmissionForCompletedSection();

        var latestResponse = await _getLatestResponseListForSubmissionQuery.GetLatestResponses(ESTABLISHMENT_ID, completeSubmission.SectionId);

        Assert.Null(latestResponse);
    }


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

            timesAnswered--;
        }
    }

    private Domain.Submissions.Models.Submission GetCompletedSubmissionForCompletedSection()
    => _submissions.First(submission => submission.Completed &&
                                        (submission.SectionId == _sections[2].Sys.Id || submission.SectionId == _sections[3].Sys.Id));

    private Domain.Submissions.Models.Submission GetIncompleteSubmissionForIncompleteSection()
    => _submissions.First(submission => !submission.Completed &&
                        (submission.SectionId == _sections[0].Sys.Id || submission.SectionId == _sections[1].Sys.Id));

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
            Maturity = answer.Maturity
        };
        return response;
    }
}