using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Exceptions;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class QuestionServiceTests
{
    private readonly ISubmissionWorkflow _mockSubmissionWorkflow = Substitute.For<ISubmissionWorkflow>();

    private static QuestionnaireSectionEntry BuildSectionGraph()
    {
        var q2 = new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails("Q2"),
            Answers = []
        };

        var a1 = new QuestionnaireAnswerEntry
        {
            Sys = new SystemDetails("A1"),
            NextQuestion = q2
        };

        var a2 = new QuestionnaireAnswerEntry
        {
            Sys = new SystemDetails("A2"),
            NextQuestion = null
        };

        var q1 = new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails("Q1"),
            Answers = new List<QuestionnaireAnswerEntry> { a1, a2 }
        };

        return new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("S1"),
            Questions = [q1, q2]
        };
    }

    private QuestionService CreateServiceUnderTest() => new QuestionService(_mockSubmissionWorkflow);

    [Fact]
    public async Task Returns_FirstQuestion_When_No_Submission()
    {
        var section = BuildSectionGraph();
        const int establishmentId = 1;

        _mockSubmissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, status: SubmissionStatus.InProgress)
           .Returns((SqlSubmissionDto?)null);

        var questionService = CreateServiceUnderTest();

        var next = await questionService.GetNextUnansweredQuestion(establishmentId, section);

        Assert.NotNull(next);
        Assert.Equal("Q1", next!.Id);

        await _mockSubmissionWorkflow.Received(1)
                 .GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, status: SubmissionStatus.InProgress);
    }

    [Fact]
    public async Task Returns_FirstQuestion_When_Submission_Inaccessible()
    {
        var section = BuildSectionGraph();
        const int establishmentId = 1;
        var submission = new SqlSubmissionDto { Status = SubmissionStatus.Inaccessible };

        _mockSubmissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, status: SubmissionStatus.InProgress)
           .Returns(submission);

        var questionService = CreateServiceUnderTest();

        var next = await questionService.GetNextUnansweredQuestion(establishmentId, section);

        Assert.NotNull(next);
        Assert.Equal("Q1", next!.Id);

        await _mockSubmissionWorkflow.Received(1)
                 .GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, status: SubmissionStatus.InProgress);
    }

    [Fact]
    public async Task Throws_When_Submission_Has_No_Responses()
    {
        var section = BuildSectionGraph();
        const int establishmentId = 1;

        var submission = new SqlSubmissionDto { Id = 1, Responses = [] };

        _mockSubmissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, status: SubmissionStatus.InProgress)
           .Returns(submission);

        var questionService = CreateServiceUnderTest();

        var ex = await Assert.ThrowsAsync<DatabaseException>(
            () => questionService.GetNextUnansweredQuestion(establishmentId, section));

        Assert.Contains("no responses", ex.Message, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(submission.Id.ToString(), ex.Message);
        Assert.Contains(establishmentId.ToString(), ex.Message);
    }

    [Fact]
    public async Task Uses_Last_Response_To_Find_NextQuestion()
    {
        var section = BuildSectionGraph();
        const int establishmentId = 1;

        var submission = new SqlSubmissionDto
        {
            Id = 1,
            Status = SubmissionStatus.InProgress,
            Responses =
            [
                new() {
                    Question = new SqlQuestionDto
                    {
                        Id = 1,
                        ContentfulSysId = "Q1"
                    },
                    Answer = new SqlAnswerDto
                    {
                        Id = 1,
                        ContentfulSysId = "A2"
                    }
                },
                new() {
                    Question = new SqlQuestionDto
                    {
                        Id = 1,
                        ContentfulSysId = "Q1"
                    },
                    Answer = new SqlAnswerDto
                    {
                        Id = 2,
                        ContentfulSysId = "A1"
                    }
                },
            ]
        };

        _mockSubmissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, status: SubmissionStatus.InProgress)
           .Returns(submission);

        var questionService = CreateServiceUnderTest();

        var next = await questionService.GetNextUnansweredQuestion(establishmentId, section);

        Assert.NotNull(next);
        Assert.Equal("Q2", next!.Id);
    }

    [Fact]
    public async Task Returns_Null_When_Response_Refs_Dont_Match_Section()
    {
        var section = BuildSectionGraph();
        const int establishmentId = 1;

        var submission = new SqlSubmissionDto
        {
            Id = 1,
            Responses =
            [
                new() {
                    Question = new SqlQuestionDto
                    {
                        Id = 999,
                        ContentfulSysId = "Q999",
                    },
                    Answer = new SqlAnswerDto
                    {
                        Id = 999,
                        ContentfulSysId = "A999"
                    }
                }
            ]
        };

        _mockSubmissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(establishmentId, section, status: SubmissionStatus.InProgress)
           .Returns(submission);

        var questionService = CreateServiceUnderTest();

        var next = await questionService.GetNextUnansweredQuestion(establishmentId, section);

        Assert.Null(next);
    }
}
