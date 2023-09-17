using System.Reflection.Metadata;
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
                                    .RuleFor(submission => submission.Responses, (faker, submission) => GenerateResponses(submission, faker).ToList());

        _submissions = submissionFaker.Generate(20);

        _planTechDbContextSubstitute = Substitute.For<IPlanTechDbContext>();
        _getLatestResponseListForSubmissionQuery = new GetLatestResponseListForSubmissionQuery(_planTechDbContextSubstitute);

        _planTechDbContextSubstitute.GetSubmissions.Returns(_submissions.AsQueryable());
    }

    [Fact]
    public void Should_Get_LatestResponse_For_QuestionId()
    {
        var responsesGroupedByQuestion = _submissions.SelectMany(submission => submission.Responses).GroupBy(r => r.Question);

        // var responsesGroupedByQuestions = _sections.SelectMany(s => s.Questions).ToDictionary(q => q.Sys.Id, q => responsesGroupedByQuestion.First(g => g.Key.ContentfulRef == q.Sys.Id));

        var tst = "";
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