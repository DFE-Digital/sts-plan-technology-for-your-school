using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class SubmissionWorkflowTests
{
    private readonly ILogger<SubmissionWorkflow> _logger = Substitute.For<
        ILogger<SubmissionWorkflow>
    >();
    private readonly IStoredProcedureRepository _sp = Substitute.For<IStoredProcedureRepository>();
    private readonly ISubmissionRepository _repo = Substitute.For<ISubmissionRepository>();
    private static readonly string[] q1q2 = new[] { "Q1", "Q2" };
    private static readonly string[] a1a2 = new[] { "A1", "A2" };

    private SubmissionWorkflow CreateServiceUnderTest() => new(_sp, _repo);

    // ---------- Helpers: minimal Contentful section graph ----------
    private static EstablishmentEntity BuildEstablishment(int? id = 1)
    {
        return new EstablishmentEntity
        {
            Id = id!.Value,
            EstablishmentRef = "testRef",
            OrgName = "testName",
            DateCreated = DateTime.UtcNow,
        };
    }

    private static QuestionnaireSectionEntry BuildSection(
        out QuestionnaireQuestionEntry q1,
        out QuestionnaireQuestionEntry q2,
        out QuestionnaireQuestionEntry q3,
        out QuestionnaireAnswerEntry a1_to_q2,
        out QuestionnaireAnswerEntry a2_to_q3,
        out QuestionnaireAnswerEntry a3_to_null
    )
    {
        q3 = new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails("Q3"),
            Answers = new List<QuestionnaireAnswerEntry>(),
        };
        a3_to_null = new QuestionnaireAnswerEntry
        {
            Sys = new SystemDetails("A3"),
            NextQuestion = null,
        };

        q2 = new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails("Q2"),
            Answers = new List<QuestionnaireAnswerEntry>(),
        };
        a2_to_q3 = new QuestionnaireAnswerEntry
        {
            Sys = new SystemDetails("A2"),
            NextQuestion = q3,
        };
        q2.Answers = new List<QuestionnaireAnswerEntry> { a2_to_q3 };

        q1 = new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails("Q1"),
            Answers = new List<QuestionnaireAnswerEntry>(),
        };
        a1_to_q2 = new QuestionnaireAnswerEntry
        {
            Sys = new SystemDetails("A1"),
            NextQuestion = q2,
        };
        q1.Answers = new List<QuestionnaireAnswerEntry> { a1_to_q2 };

        return new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("SEC"),
            Questions = new List<QuestionnaireQuestionEntry> { q1, q2, q3 },
        };
    }

    // Allows a quick workaround for self-referential submissions, which causes an endless loop when mapping to DTO.
    private static SubmissionEntity BuildEmptySubmission()
    {
        return new SubmissionEntity
        {
            Id = 0,
            SectionId = "emptyId",
            SectionName = "emptyName",
            EstablishmentId = 1,
            Establishment = BuildEstablishment(),
            Status = SubmissionStatus.CompleteReviewed,
            Responses = [],
        };
    }

    private static ResponseEntity BuildResponse(
        string questionReference,
        string answerReference,
        DateTime dateCreated,
        int id,
        SubmissionEntity submission
    ) =>
        new ResponseEntity
        {
            Id = id,
            DateCreated = dateCreated,
            DateLastUpdated = dateCreated,
            QuestionId = id * 10,
            Question = new QuestionEntity { ContentfulRef = questionReference },
            Answer = new AnswerEntity { ContentfulRef = answerReference },
            Submission = submission,
        };

    [Fact]
    public async Task CloneLatestCompletedSubmission_Clones_And_Orders_Responses_By_Journey()
    {
        var sut = CreateServiceUnderTest();

        var section = BuildSection(
            out var q1,
            out var q2,
            out var q3,
            out var a1,
            out var a2,
            out _
        );
        var emptySubmission = BuildEmptySubmission();

        // Latest completed submission (source to clone)
        var latestCompleted = new SubmissionEntity
        {
            Id = 10,
            SectionId = section.Id,
            SectionName = "testName",
            EstablishmentId = 1,
            Establishment = BuildEstablishment(),
            Status = SubmissionStatus.CompleteReviewed,
            Responses = new List<ResponseEntity>(),
        };

        // Cloned submission comes back with responses out of order and with duplicates by question;
        // ordering should pick the *latest per question* by DateCreated, then follow Q->A.NextQuestion chain.
        var now = DateTime.UtcNow;
        var clone = new SubmissionEntity
        {
            Id = 99,
            SectionId = section.Id,
            SectionName = "testName",
            EstablishmentId = 1,
            Establishment = BuildEstablishment(),
            Status = SubmissionStatus.InProgress,
            Responses = new List<ResponseEntity>
            {
                // Q1 older A1 (should be ignored because newer exists)
                BuildResponse(q1.Id, a1.Id, now.AddMinutes(-10), 1, emptySubmission),
                // Q2 A2
                BuildResponse(q2.Id, a2.Id, now.AddMinutes(-5), 2, emptySubmission),
                // Q1 newer A1 (should be selected for Q1)
                BuildResponse(q1.Id, a1.Id, now.AddMinutes(-1), 3, emptySubmission),
            },
        };

        _repo
            .GetLatestSubmissionAndResponsesAsync(
                123,
                section.Id,
                status: SubmissionStatus.CompleteReviewed
            )
            .Returns(latestCompleted);
        _repo.CloneSubmission(latestCompleted).Returns(clone);

        var dto = await sut.CloneLatestCompletedSubmission(123, section);

        // Expect path: Q1 (latest) -> Q2 -> Q3 (no response for Q3 so stop after Q2)
        Assert.Equal(q1q2, dto.Responses.Select(r => r.Question.ContentfulSysId).ToArray());
        Assert.Equal(a1a2, dto.Responses.Select(r => r.Answer.ContentfulSysId).ToArray());

        await _repo.Received(1).CloneSubmission(latestCompleted);
    }

    // ---------- GetLatestSubmissionWithOrderedResponsesAsync ----------
    [Fact]
    public async Task GetLatestSubmissionWithOrderedResponses_Returns_Null_When_None()
    {
        var sut = CreateServiceUnderTest();
        var section = BuildSection(out _, out _, out _, out _, out _, out _);

        _repo
            .GetLatestSubmissionAndResponsesAsync(1, section.Id, status: (SubmissionStatus?)null)
            .Returns((SubmissionEntity?)null);

        var dto = await sut.GetLatestSubmissionWithOrderedResponsesAsync(1, section, null);
        Assert.Null(dto);
    }

    [Fact]
    public async Task GetLatestSubmissionWithOrderedResponses_Orders_Then_Returns_Dto()
    {
        var sut = CreateServiceUnderTest();
        var section = BuildSection(
            out var q1,
            out var q2,
            out var q3,
            out var a1,
            out var a2,
            out _
        );
        var now = DateTime.UtcNow;

        var emptySubmission = BuildEmptySubmission();

        var submission = new SubmissionEntity
        {
            Id = 55,
            SectionId = section.Id,
            SectionName = "testName",
            Maturity = "developing",
            EstablishmentId = 1,
            Establishment = BuildEstablishment(),
            Status = SubmissionStatus.InProgress,
            Responses = new List<ResponseEntity>
            {
                BuildResponse(q2.Id, a2.Id, now.AddMinutes(-1), 22, emptySubmission),
                BuildResponse(q1.Id, a1.Id, now.AddMinutes(-2), 11, emptySubmission),
            },
        };

        _repo
            .GetLatestSubmissionAndResponsesAsync(5, section.Id, SubmissionStatus.InProgress)
            .Returns(submission);

        var dto = await sut.GetLatestSubmissionWithOrderedResponsesAsync(
            5,
            section,
            SubmissionStatus.InProgress
        );

        Assert.NotNull(dto);
        Assert.Equal(2, dto!.Responses.Count());
        // Should start at Q1 then go to Q2 per chain
        Assert.Equal(q1q2, dto.Responses.Select(r => r.Question.ContentfulSysId).ToArray());
        Assert.Equal("developing", dto.Maturity);
    }

    // ---------- SubmitAnswer ----------
    [Fact]
    public async Task SubmitAnswer_Throws_When_Model_Null()
    {
        var sut = CreateServiceUnderTest();
        await Assert.ThrowsAsync<InvalidDataException>(() => sut.SubmitAnswer(1, 2, 2, null!));
    }

    [Fact]
    public async Task SubmitAnswer_Calls_SP_With_AssessmentResponseModel_And_Returns_Id()
    {
        var sut = CreateServiceUnderTest();
        var questionModel = new IdWithTextModel { Id = "Q1", Text = "Question 1 text" };
        var answerModel = new IdWithTextModel { Id = "A1", Text = "Answer 1 text" };
        var model = new SubmitAnswerModel { Question = questionModel, ChosenAnswer = answerModel };

        _sp.SubmitResponse(
                Arg.Is<AssessmentResponseModel>(m =>
                    m.UserId == 9
                    && m.EstablishmentId == 8
                    && m.UserEstablishmentId == 7
                    && m.Question.Id == "Q1"
                    && m.Answer!.Id == "A1"
                )
            )
            .Returns(777);

        // activeEstablishmentId = 8, userEstablishmentId = 7 (simulating MAT user selecting a school)
        var id = await sut.SubmitAnswer(9, 8, 7, model);

        Assert.Equal(777, id);
    }

    // ---------- GetSectionStatusesAsync ----------
    [Fact]
    public async Task GetSectionStatuses_Joins_Ids_And_Maps_Dtos()
    {
        var sut = CreateServiceUnderTest();
        var entities = new List<SectionStatusEntity>
        {
            new()
            {
                SectionId = "S1",
                Status = SubmissionStatus.CompleteReviewed,
                LastMaturity = "developing",
            },
            new()
            {
                SectionId = "S2",
                Status = SubmissionStatus.NotStarted,
                LastMaturity = null,
            },
        };

        _sp.GetSectionStatusesAsync("S1,S2", 123).Returns(entities);

        var result = await sut.GetSectionStatusesAsync(123, ["S1", "S2"]);

        Assert.Collection(
            result,
            s =>
            {
                Assert.Equal("S1", s.SectionId);
                Assert.Equal(SubmissionStatus.CompleteReviewed, s.Status);
                Assert.Equal("developing", s.LastMaturity);
            },
            s =>
            {
                Assert.Equal("S2", s.SectionId);
                Assert.Equal(SubmissionStatus.NotStarted, s.Status);
                Assert.Null(s.LastMaturity);
            }
        );

        await _sp.Received(1).GetSectionStatusesAsync("S1,S2", 123);
    }

    // ---------- GetSectionSubmissionStatusAsync ----------
    [Fact]
    public async Task GetSectionSubmissionStatus_When_Found_Completed_True()
    {
        var sut = CreateServiceUnderTest();
        var sub = new SubmissionEntity
        {
            SectionId = "S1",
            SectionName = "testName",
            Maturity = "high",
            EstablishmentId = 1,
            Establishment = BuildEstablishment(),
            Status = SubmissionStatus.CompleteReviewed,
        };

        _repo
            .GetLatestSubmissionAndResponsesAsync(1, "SEC", SubmissionStatus.CompleteReviewed)
            .Returns(sub);

        var dto = await sut.GetSectionSubmissionStatusAsync(
            1,
            "SEC",
            SubmissionStatus.CompleteReviewed
        );

        Assert.Equal("high", dto.LastMaturity);
        Assert.Equal(SubmissionStatus.CompleteReviewed, dto.Status);
    }

    [Fact]
    public async Task GetSectionSubmissionStatus_When_Found_Completed_False()
    {
        var sut = CreateServiceUnderTest();
        var sub = new SubmissionEntity
        {
            SectionId = "S1",
            SectionName = "testName",
            Maturity = "high",
            EstablishmentId = 1,
            Establishment = BuildEstablishment(),
            Status = SubmissionStatus.InProgress,
        };

        _repo
            .GetLatestSubmissionAndResponsesAsync(1, "SEC", SubmissionStatus.InProgress)
            .Returns(sub);

        var dto = await sut.GetSectionSubmissionStatusAsync(1, "SEC", SubmissionStatus.InProgress);

        Assert.Equal("high", dto.LastMaturity);
        Assert.Equal(SubmissionStatus.InProgress, dto.Status);
    }

    [Fact]
    public async Task GetSectionSubmissionStatus_When_None_NotStarted()
    {
        var sut = CreateServiceUnderTest();
        _repo
            .GetLatestSubmissionAndResponsesAsync(1, "SEC", SubmissionStatus.CompleteReviewed)
            .Returns((SubmissionEntity?)null);

        var dto = await sut.GetSectionSubmissionStatusAsync(
            1,
            "SEC",
            SubmissionStatus.CompleteReviewed
        );

        Assert.Equal("SEC", dto.SectionId);
        Assert.Equal(SubmissionStatus.NotStarted, dto.Status);
    }

    // ---------- Set / Delete delegations ----------
    [Fact]
    public async Task SetMaturityAndMarkAsReviewed_Calls_SP_Then_Repo()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetMaturityAndMarkAsReviewedAsync(42);
        await _sp.Received(1).SetMaturityForSubmissionAsync(42);
        await _repo
            .Received(1)
            .SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(42);
    }

    [Fact]
    public async Task SetLatestSubmissionViewed_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetLatestSubmissionViewedAsync(9, "SEC");
        await _repo.Received(1).SetLatestSubmissionViewedAsync(9, "SEC");
    }

    [Fact]
    public async Task SetSubmissionReviewed_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionReviewedAsync(99);
        await _repo
            .Received(1)
            .SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(99);
    }

    [Fact]
    public async Task SetSubmissionInaccessible_By_Establishment_Section_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionInaccessibleAsync(1, "SEC");
        await _repo.Received(1).SetSubmissionInaccessibleAsync(1, "SEC");
    }

    [Fact]
    public async Task SetSubmissionInaccessible_By_SubmissionId_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionInaccessibleAsync(123);
        await _repo.Received(1).SetSubmissionInaccessibleAsync(123);
    }

    [Fact]
    public async Task SetSubmissionInProgress_By_Establishment_Section_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionInProgressAsync(1, "SEC");
        await _repo.Received(1).SetSubmissionInProgressAsync(1, "SEC");
    }

    [Fact]
    public async Task SetSubmissionInProgress_By_SubmissionId_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionInProgressAsync(123);
        await _repo.Received(1).SetSubmissionInProgressAsync(123);
    }

    [Fact]
    public async Task SetSubmissionDeleted_Delegates_To_SP()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionDeletedAsync(1, "SEC");
        await _sp.Received(1).SetSubmissionDeletedAsync(1, "SEC");
    }

    [Fact]
    public async Task ConfirmCheckAnswersAndUpdateRecommendationsAsync_CallsRepository()
    {
        var sut = CreateServiceUnderTest();
        var section = new QuestionnaireSectionEntry();

        await sut.ConfirmCheckAnswersAndUpdateRecommendationsAsync(1, 1, 123, 99, section);
        await _repo
            .Received(1)
            .ConfirmCheckAnswersAndUpdateRecommendationsAsync(1, 1, 123, 99, section);
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_CallsRepo()
    {
        var sut = CreateServiceUnderTest();
        var submission = new SubmissionEntity
        {
            Id = 444,
            SectionId = "secId",
            SectionName = "testSection",
            EstablishmentId = 1,
            Establishment = BuildEstablishment(),
            Status = SubmissionStatus.CompleteReviewed,
            Responses = new List<ResponseEntity>(),
        };

        _repo.GetSubmissionByIdAsync(submission.Id).Returns(submission);

        await sut.GetSubmissionByIdAsync(submission.Id);

        await _repo.Received(1).GetSubmissionByIdAsync(submission.Id);
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_ReturnsSubmissionDto()
    {
        var submission = new SubmissionEntity
        {
            Id = 555,
            SectionId = "secId",
            SectionName = "testSection",
            EstablishmentId = 1,
            Establishment = BuildEstablishment(),
            Status = SubmissionStatus.CompleteReviewed,
            Responses = new List<ResponseEntity>(),
        };

        var sut = CreateServiceUnderTest();

        _repo.GetSubmissionByIdAsync(555).Returns(submission);

        var dto = await sut.GetSubmissionByIdAsync(555);

        Assert.NotNull(dto);
        Assert.Equal(submission.Id, dto.Id);
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_ThrowsWhenNullSubmission()
    {
        var sut = CreateServiceUnderTest();
        SubmissionEntity nullSubmission = null!;
        var submissionId = 666;

        _repo.GetSubmissionByIdAsync(666).Returns(nullSubmission);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GetSubmissionByIdAsync(submissionId)
        );
        Assert.Equal($"Submission with ID '{submissionId}' not found", ex.Message);
    }
}
