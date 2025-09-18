using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class SubmissionServiceTests
{
    private readonly ISubmissionWorkflow _submissionWorkflow = Substitute.For<ISubmissionWorkflow>();

    private SubmissionService CreateServiceUnderTest() => new SubmissionService(_submissionWorkflow);

    private static (QuestionnaireSectionEntry section,
                    QuestionnaireQuestionEntry q1,
                    QuestionnaireQuestionEntry q2,
                    QuestionnaireAnswerEntry a1_to_q2,
                    QuestionnaireAnswerEntry a2_to_null)
    BuildSectionGraph()
    {
        var q2 = new QuestionnaireQuestionEntry { Sys = new SystemDetails("2"), Answers = new List<QuestionnaireAnswerEntry>() };
        var a1 = new QuestionnaireAnswerEntry { Sys = new SystemDetails("1"), NextQuestion = q2 };
        var a2 = new QuestionnaireAnswerEntry { Sys = new SystemDetails("2"), NextQuestion = null };
        var q1 = new QuestionnaireQuestionEntry { Sys = new SystemDetails("1"), Answers = new List<QuestionnaireAnswerEntry> { a1, a2 } };

        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("S1"),
            Questions = new List<QuestionnaireQuestionEntry> { q1, q2 }
        };

        return (section, q1, q2, a1, a2);
    }

    private static SqlSubmissionDto SubmissionWithResponses(bool completed, string? maturity, params (string qId, string aId)[] responses)
    {
        return new SqlSubmissionDto
        {
            Id = 1,
            Completed = completed,
            Maturity = maturity,
            Status = null, // let routing derive from last answer path
            Responses = responses.Select(r => new SqlResponseDto
            {
                Question = new SqlQuestionDto
                {
                    Id = int.Parse(r.qId),
                    ContentfulSysId = r.qId
                },
                Answer = new SqlAnswerDto
                {
                    Id = int.Parse(r.aId),
                    ContentfulSysId = r.aId
                }
            }).ToList()
        };
    }

    // --- RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync -----------

    [Fact]
    public async Task RemoveAndClone_When_InProgressExists_Deletes_Then_Clones()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, _) = BuildSectionGraph();

        var inProgress = new SqlSubmissionDto { Id = 100, Completed = false, Responses = new List<SqlResponseDto> { new() { QuestionId = 1, AnswerId = 1 } } };
        var cloned = new SqlSubmissionDto { Id = 200, Completed = true };

        _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(123, section, isCompletedSubmission: false)
           .Returns(inProgress);

        _submissionWorkflow.CloneLatestCompletedSubmission(123, section).Returns(cloned);

        var result = await sut.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(123, section);

        Assert.Same(cloned, result);

        await _submissionWorkflow.Received(1).DeleteSubmissionSoftAsync(inProgress.Id);
        await _submissionWorkflow.Received(1).CloneLatestCompletedSubmission(123, section);
    }

    [Fact]
    public async Task RemoveAndClone_When_No_InProgress_Just_Clones()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, _) = BuildSectionGraph();

        _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(123, section, false)
           .Returns((SqlSubmissionDto?)null);

        var cloned = new SqlSubmissionDto { Id = 201, Completed = true };
        _submissionWorkflow.CloneLatestCompletedSubmission(123, section).Returns(cloned);

        var result = await sut.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(123, section);

        Assert.Same(cloned, result);
        await _submissionWorkflow.DidNotReceive().DeleteSubmissionSoftAsync(Arg.Any<int>());
        await _submissionWorkflow.Received(1).CloneLatestCompletedSubmission(123, section);
    }

    [Fact]
    public async Task GetLatestSubmissionResponsesModel_Returns_Null_When_No_Submission()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, _) = BuildSectionGraph();

        _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(1, section, true)
           .Returns((SqlSubmissionDto?)null);

        var model = await sut.GetLatestSubmissionResponsesModel(1, section, isCompletedSubmission: true);

        Assert.Null(model);
    }

    [Fact]
    public async Task GetLatestSubmissionResponsesModel_Wraps_Submission()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, _) = BuildSectionGraph();

        var sub = SubmissionWithResponses(completed: false, maturity: "medium", ("1", "1"));
        _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(1, section, false)
           .Returns(sub);

        var model = await sut.GetLatestSubmissionResponsesModel(1, section, isCompletedSubmission: false);

        Assert.NotNull(model);
        Assert.Equal("medium", model!.Maturity);
        Assert.Equal("1", model.Responses.Last().QuestionSysId);
        Assert.Equal("1", model.Responses.Last().AnswerSysId);
    }

    [Fact]
    public async Task Routing_NotStarted_When_No_Submission()
    {
        var sut = CreateServiceUnderTest();
        var (section, q1, _, _, _) = BuildSectionGraph();

        _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(10, section, null)
           .Returns((SqlSubmissionDto?)null);

        var rd = await sut.GetSubmissionRoutingDataAsync(10, section, isCompletedSubmission: null);

        Assert.Equal(SubmissionStatus.NotStarted, rd.Status);
        Assert.Same(q1, rd.NextQuestion); // first question
        Assert.Same(section, rd.QuestionnaireSection);
        Assert.Null(rd.Submission);
    }

    [Fact]
    public async Task Routing_InProgress_When_Last_Answer_Points_To_NextQuestion()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, q2, a1_to_q2, _) = BuildSectionGraph();

        var sub = SubmissionWithResponses(completed: false, maturity: "medium",
                                          ("1", a1_to_q2.Id)); // last answer -> Q2

        _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(22, section, null)
           .Returns(sub);

        var rd = await sut.GetSubmissionRoutingDataAsync(22, section, isCompletedSubmission: null);

        Assert.Equal(SubmissionStatus.InProgress, rd.Status);
        Assert.Same(q2, rd.NextQuestion);
        Assert.NotNull(rd.Submission);
        // also verify pass-through arg:
        await _submissionWorkflow.Received(1).GetLatestSubmissionWithOrderedResponsesAsync(22, section, null);
    }

    [Fact]
    public async Task Routing_CompleteNotReviewed_When_No_NextQuestion_And_Status_Null()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, a2_to_null) = BuildSectionGraph();

        var sub = SubmissionWithResponses(completed: true, maturity: "secure",
                                          ("1", a2_to_null.Id)); // last answer -> null

        _submissionWorkflow.GetLatestSubmissionWithOrderedResponsesAsync(33, section, null)
           .Returns(sub);

        var rd = await sut.GetSubmissionRoutingDataAsync(33, section, isCompletedSubmission: null);

        Assert.Equal(SubmissionStatus.CompleteNotReviewed, rd.Status);
        Assert.Null(rd.NextQuestion);
    }
}
