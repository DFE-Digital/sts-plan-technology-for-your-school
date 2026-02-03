using Dfe.PlanTech.Application.Services;
using Dfe.PlanTech.Application.Workflows.Interfaces;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Services;

public class SubmissionServiceTests
{
    private readonly IRecommendationWorkflow _recommendationWorkflow =
        Substitute.For<IRecommendationWorkflow>();
    private readonly ISubmissionWorkflow _submissionWorkflow =
        Substitute.For<ISubmissionWorkflow>();

    private SubmissionService CreateServiceUnderTest() =>
        new SubmissionService(_recommendationWorkflow, _submissionWorkflow);

    private static (
        QuestionnaireSectionEntry section,
        QuestionnaireQuestionEntry q1,
        QuestionnaireQuestionEntry q2,
        QuestionnaireAnswerEntry a1_to_q2,
        QuestionnaireAnswerEntry a2_to_null
    ) BuildSectionGraph()
    {
        var q2 = new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails("2"),
            Answers = new List<QuestionnaireAnswerEntry>(),
        };
        var a1 = new QuestionnaireAnswerEntry { Sys = new SystemDetails("1"), NextQuestion = q2 };
        var a2 = new QuestionnaireAnswerEntry { Sys = new SystemDetails("2"), NextQuestion = null };
        var q1 = new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails("1"),
            Answers = new List<QuestionnaireAnswerEntry> { a1, a2 },
        };

        var section = new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("S1"),
            Questions = new List<QuestionnaireQuestionEntry> { q1, q2 },
        };

        return (section, q1, q2, a1, a2);
    }

    private static SqlSubmissionDto SubmissionWithResponses(
        bool completed,
        string? maturity,
        params (string qId, string aId)[] responses
    )
    {
        return new SqlSubmissionDto
        {
            Id = 1,
            Maturity = maturity,
            Status = SubmissionStatus.None,
            Responses = responses
                .Select(r => new SqlResponseDto
                {
                    Question = new SqlQuestionDto
                    {
                        Id = int.Parse(r.qId),
                        ContentfulSysId = r.qId,
                    },
                    Answer = new SqlAnswerDto { Id = int.Parse(r.aId), ContentfulSysId = r.aId },
                })
                .ToList(),
        };
    }

    // --- RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync -----------

    [Fact]
    public async Task RemoveAndClone_When_InProgressExists_Clones_And_MarksPreviousInaccessible()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, _) = BuildSectionGraph();

        var inProgress = new SqlSubmissionDto
        {
            Id = 100,
            Status = SubmissionStatus.InProgress,
            Responses = new List<SqlResponseDto>
            {
                new() { QuestionId = 1, AnswerId = 1 },
            },
        };
        var cloned = new SqlSubmissionDto { Id = 200, Status = SubmissionStatus.CompleteReviewed };

        _submissionWorkflow
            .GetLatestSubmissionWithOrderedResponsesAsync(
                123,
                section.Id,
                status: SubmissionStatus.InProgress
            )
            .Returns(inProgress);

        _submissionWorkflow.CloneLatestCompletedSubmission(123, section.Id).Returns(cloned);

        var result = await sut.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(
            123,
            section.Id
        );

        Assert.Same(cloned, result);

        await _submissionWorkflow.Received(1).CloneLatestCompletedSubmission(123, section.Id);
        await _submissionWorkflow.Received(1).SetSubmissionInaccessibleAsync(inProgress.Id);
    }

    [Fact]
    public async Task MakeInaccessibleAndClone_When_No_InProgress_Just_Clones()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, _) = BuildSectionGraph();

        _submissionWorkflow
            .GetLatestSubmissionWithOrderedResponsesAsync(
                123,
                section.Id,
                status: SubmissionStatus.InProgress
            )
            .Returns((SqlSubmissionDto?)null);

        var cloned = new SqlSubmissionDto { Id = 201, Status = SubmissionStatus.CompleteReviewed };
        _submissionWorkflow.CloneLatestCompletedSubmission(123, section.Id).Returns(cloned);

        var result = await sut.RemovePreviousSubmissionsAndCloneMostRecentCompletedAsync(
            123,
            section.Id
        );

        Assert.Same(cloned, result);
        await _submissionWorkflow.Received(1).CloneLatestCompletedSubmission(123, section.Id);
        await _submissionWorkflow.DidNotReceive().SetSubmissionInaccessibleAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task GetLatestSubmissionResponsesModel_Returns_Null_When_No_Submission()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, _) = BuildSectionGraph();

        _submissionWorkflow
            .GetLatestSubmissionWithOrderedResponsesAsync(
                1,
                section.Id,
                status: SubmissionStatus.CompleteReviewed
            )
            .Returns((SqlSubmissionDto?)null);

        var model = await sut.GetLatestSubmissionResponsesModel(
            1,
            section,
            status: SubmissionStatus.CompleteReviewed
        );

        Assert.Null(model);
    }

    [Fact]
    public async Task GetLatestSubmissionResponsesModel_Wraps_Submission()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, _) = BuildSectionGraph();

        var sub = SubmissionWithResponses(completed: false, maturity: "medium", ("1", "1"));
        _submissionWorkflow
            .GetLatestSubmissionWithOrderedResponsesAsync(
                1,
                section.Id,
                SubmissionStatus.InProgress
            )
            .Returns(sub);

        var model = await sut.GetLatestSubmissionResponsesModel(
            1,
            section,
            status: SubmissionStatus.InProgress
        );

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

        _submissionWorkflow
            .GetLatestSubmissionWithOrderedResponsesAsync(10, section.Id, status: null)
            .Returns((SqlSubmissionDto?)null);

        var rd = await sut.GetSubmissionRoutingDataAsync(10, section, status: null);

        Assert.Equal(SubmissionStatus.NotStarted, rd.Status);
        Assert.Same(q1, rd.NextQuestion);
        Assert.Same(section, rd.QuestionnaireSection);
        Assert.Null(rd.Submission);
    }

    [Fact]
    public async Task Routing_InProgress_When_Last_Answer_Points_To_NextQuestion()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, q2, a1_to_q2, _) = BuildSectionGraph();

        var sub = SubmissionWithResponses(completed: false, maturity: "medium", ("1", a1_to_q2.Id));

        _submissionWorkflow
            .GetLatestSubmissionWithOrderedResponsesAsync(22, section.Id, status: null)
            .Returns(sub);

        var rd = await sut.GetSubmissionRoutingDataAsync(22, section, status: null);

        Assert.Equal(SubmissionStatus.InProgress, rd.Status);
        Assert.Same(q2, rd.NextQuestion);
        Assert.NotNull(rd.Submission);
        // also verify pass-through arg:
        await _submissionWorkflow
            .Received(1)
            .GetLatestSubmissionWithOrderedResponsesAsync(22, section.Id, status: null);
    }

    [Fact]
    public async Task Routing_CompleteNotReviewed_When_No_NextQuestion_And_Status_Null()
    {
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, a2_to_null) = BuildSectionGraph();

        var sub = SubmissionWithResponses(
            completed: true,
            maturity: "secure",
            ("1", a2_to_null.Id)
        );

        _submissionWorkflow
            .GetLatestSubmissionWithOrderedResponsesAsync(33, section.Id, status: null)
            .Returns(sub);

        var rd = await sut.GetSubmissionRoutingDataAsync(33, section, status: null);

        Assert.Equal(SubmissionStatus.CompleteNotReviewed, rd.Status);
        Assert.Null(rd.NextQuestion);
    }

    [Fact]
    public async Task Routing_Next_Question_When_Status_InProgress()
    {
        // Arrange
        var sut = CreateServiceUnderTest();
        var (section, _, _, _, _) = BuildSectionGraph();

        var sub = SubmissionWithResponses(completed: false, maturity: "medium", ("1", "1"));
        sub.Status = SubmissionStatus.InProgress;

        _submissionWorkflow
            .GetLatestSubmissionWithOrderedResponsesAsync(
                44,
                section.Id,
                status: SubmissionStatus.InProgress
            )
            .Returns(sub);

        // Act
        var rd = await sut.GetSubmissionRoutingDataAsync(
            44,
            section,
            status: SubmissionStatus.InProgress
        );

        // Assert
        Assert.Equal(SubmissionStatus.InProgress, rd.Status);
        Assert.NotNull(rd.NextQuestion);
        Assert.Equal("2", rd.NextQuestion.Id);
    }

    [Fact]
    public async Task GetSectionStatusesForSchoolAsync_Calls_Workflow_And_Returns_Result()
    {
        var sut = CreateServiceUnderTest();
        var expected = new List<SqlSectionStatusDto> { new SqlSectionStatusDto() };

        _submissionWorkflow
            .GetSectionStatusesAsync(123, Arg.Any<IEnumerable<string>>())
            .Returns(expected);

        var result = await sut.GetSectionStatusesForSchoolAsync(123, new[] { "sec1", "sec2" });

        Assert.Same(expected, result);
        await _submissionWorkflow
            .Received(1)
            .GetSectionStatusesAsync(
                123,
                Arg.Is<IEnumerable<string>>(ids => ids.Contains("sec1") && ids.Contains("sec2"))
            );
    }

    [Fact]
    public async Task SetLatestSubmissionViewedAsync_Calls_Workflow()
    {
        var sut = CreateServiceUnderTest();

        await sut.SetLatestSubmissionViewedAsync(123, "sec1");

        await _submissionWorkflow.Received(1).SetLatestSubmissionViewedAsync(123, "sec1");
    }

    [Fact]
    public async Task SubmitAnswerAsync_Calls_Workflow_And_Returns_Result()
    {
        var sut = CreateServiceUnderTest();
        var expected = 42;
        var model = new SubmitAnswerModel();

        _submissionWorkflow.SubmitAnswer(1, 2, 3, model).Returns(expected);

        var result = await sut.SubmitAnswerAsync(1, 2, 3, model);

        Assert.Equal(expected, result);
        await _submissionWorkflow.Received(1).SubmitAnswer(1, 2, 3, model);
    }

    [Fact]
    public async Task ConfirmCheckAnswersAsync_Calls_Workflow()
    {
        var sut = CreateServiceUnderTest();

        await sut.ConfirmCheckAnswersAsync(999);

        await _submissionWorkflow.Received(1).SetMaturityAndMarkAsReviewedAsync(999);
    }

    [Fact]
    public async Task ConfirmCheckAnswersAndUpdateRecommendationsAsync_Calls_Workflow()
    {
        var sut = CreateServiceUnderTest();
        var section = new QuestionnaireSectionEntry();

        await sut.ConfirmCheckAnswersAndUpdateRecommendationsAsync(1, 2, 3, 4, section);

        await _submissionWorkflow
            .Received(1)
            .ConfirmCheckAnswersAndUpdateRecommendationsAsync(1, 2, 3, 4, section);
    }

    [Fact]
    public async Task SetSubmissionInaccessibleAsync_Calls_Workflow()
    {
        var sut = CreateServiceUnderTest();

        await sut.SetSubmissionInaccessibleAsync(123, "sec1");

        await _submissionWorkflow.Received(1).SetSubmissionInaccessibleAsync(123, "sec1");
    }

    [Fact]
    public async Task RestoreInaccessibleSubmissionAsync_Calls_Workflow()
    {
        var sut = CreateServiceUnderTest();

        await sut.RestoreInaccessibleSubmissionAsync(123, "sec1");

        await _submissionWorkflow.Received(1).SetSubmissionInProgressAsync(123, "sec1");
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_Calls_Workflow_And_Returns_Dto()
    {
        var sut = CreateServiceUnderTest();
        var expected = new SqlSubmissionDto { Id = 1, EstablishmentId = 99 };
        var model = new SubmitAnswerModel();

        _submissionWorkflow.GetSubmissionByIdAsync(101).Returns(expected);

        var result = await sut.GetSubmissionByIdAsync(101);

        Assert.Equal(expected, result);
        await _submissionWorkflow.Received(1).GetSubmissionByIdAsync(101);
    }
}
