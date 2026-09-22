using Dfe.PlanTech.Application.Workflows;
using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Models;
using Dfe.PlanTech.Core.Providers.Interfaces;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Interfaces;
using Dfe.PlanTech.UnitTests.Shared.Builders;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Workflows;

public class SubmissionWorkflowTests
{
    private readonly ILogger<SubmissionWorkflow> _logger = Substitute.For<
        ILogger<SubmissionWorkflow>
    >();

    private readonly IAnswerRepository _answerRepository = Substitute.For<IAnswerRepository>();
    private readonly IEstablishmentRecommendationHistoryRepository _establishmentRecommendationHistoryRepository =
        Substitute.For<IEstablishmentRecommendationHistoryRepository>();
    private readonly IQuestionRepository _questionRepository =
        Substitute.For<IQuestionRepository>();
    private readonly IRecommendationRepository _recommendationRepository =
        Substitute.For<IRecommendationRepository>();
    private readonly IResponseRepository _responseRepository =
        Substitute.For<IResponseRepository>();
    private readonly ISubmissionRepository _submissionRepository =
        Substitute.For<ISubmissionRepository>();
    private readonly IUserActionIdProvider _userActionIdProvider =
        Substitute.For<IUserActionIdProvider>();
    private static readonly string[] q1q2 = ["Q1", "Q2"];
    private static readonly string[] a1a2 = ["A1", "A2"];

    private SubmissionWorkflow CreateServiceUnderTest() =>
        new(
            _answerRepository,
            _establishmentRecommendationHistoryRepository,
            _questionRepository,
            _recommendationRepository,
            _responseRepository,
            _submissionRepository,
            _userActionIdProvider
        );

    // ---------- Helpers: minimal Contentful section graph ----------

    private static ResponseEntity BuildResponse(
        int id,
        DateTime dateCreated,
        SubmissionEntity submission,
        string questionReference,
        string answerReference
    ) =>
        new()
        {
            Id = id,
            DateCreated = dateCreated,
            DateLastUpdated = dateCreated,
            QuestionId = id * 10,
            Question = new QuestionEntity { ContentfulRef = questionReference },
            Answer = new AnswerEntity { ContentfulRef = answerReference },
            Submission = submission,
        };

    private static QuestionnaireSectionEntry BuildSection(
        out QuestionnaireQuestionEntry q1,
        out QuestionnaireQuestionEntry q2,
        out QuestionnaireQuestionEntry q3,
        out QuestionnaireAnswerEntry a1_to_q2,
        out QuestionnaireAnswerEntry a2_to_q3,
        out QuestionnaireAnswerEntry a3_to_null
    )
    {
        q3 = new QuestionnaireQuestionEntry { Sys = new SystemDetails("Q3"), Answers = [] };
        a3_to_null = new QuestionnaireAnswerEntry
        {
            Sys = new SystemDetails("A3"),
            NextQuestion = null,
        };

        q2 = new QuestionnaireQuestionEntry { Sys = new SystemDetails("Q2"), Answers = [] };
        a2_to_q3 = new QuestionnaireAnswerEntry
        {
            Sys = new SystemDetails("A2"),
            NextQuestion = q3,
        };
        q2.Answers = [a2_to_q3];

        q1 = new QuestionnaireQuestionEntry { Sys = new SystemDetails("Q1"), Answers = [] };
        a1_to_q2 = new QuestionnaireAnswerEntry
        {
            Sys = new SystemDetails("A1"),
            NextQuestion = q2,
        };
        q1.Answers = [a1_to_q2];

        return new QuestionnaireSectionEntry
        {
            Sys = new SystemDetails("SEC"),
            Questions = [q1, q2, q3],
        };
    }

    // ---------- CloneLatestCompletedSubmission ----------

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
        var emptySubmission = EntityBuilders.BuildEmptySubmission();

        // Latest completed submission (source to clone)
        var latestCompleted = EntityBuilders.BuildSubmission(
            id: 10,
            establishmentId: 1,
            sectionId: "SEC11"
        );

        // Cloned submission comes back with responses out of order and with duplicates by question;
        // ordering should pick the *latest per question* by DateCreated, then follow Q->A.NextQuestion chain.
        var now = DateTime.UtcNow;
        var responses = new List<ResponseEntity>()
        {
            BuildResponse(1, now.AddMinutes(-10), emptySubmission, q1.Id, a1.Id),
            BuildResponse(1, now.AddMinutes(-5), emptySubmission, q2.Id, a2.Id),
            BuildResponse(1, now.AddMinutes(-1), emptySubmission, q1.Id, a1.Id),
        };

        var clone = EntityBuilders.BuildSubmission(
            id: 99,
            establishmentId: 1,
            sectionId: "SEC11",
            responses: responses,
            submissionStatus: SubmissionStatus.InProgress
        );

        _submissionRepository
            .GetLatestSubmissionAndResponsesAsync(
                123,
                section.Id,
                status: SubmissionStatus.CompleteReviewed
            )
            .Returns(latestCompleted);

        _submissionRepository.CloneSubmission(latestCompleted).Returns(clone);

        var dto = await sut.CloneLatestCompletedSubmission(123, section.Id);

        // Expect path: Q1 (latest) -> Q2 -> Q3 (no response for Q3 so stop after Q2)
        Assert.Equal(q1q2, dto.Responses.Select(r => r.Question.ContentfulSysId).ToArray());
        Assert.Equal(a1a2, dto.Responses.Select(r => r.Answer.ContentfulSysId).ToArray());

        await _submissionRepository.Received(1).CloneSubmission(latestCompleted);
    }

    // ---------- GetLatestSubmissionWithOrderedResponsesAsync ----------
    [Fact]
    public async Task GetLatestSubmissionWithOrderedResponses_Returns_Null_When_None()
    {
        var sut = CreateServiceUnderTest();
        var section = BuildSection(out _, out _, out _, out _, out _, out _);

        _submissionRepository
            .GetLatestSubmissionAndResponsesAsync(1, section.Id, status: (SubmissionStatus?)null)
            .Returns((SubmissionEntity?)null);

        var dto = await sut.GetLatestSubmissionWithOrderedResponsesAsync(
            1,
            section.Id,
            (SubmissionStatus?)null
        );
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

        var emptySubmission = EntityBuilders.BuildEmptySubmission();

        var responses = new List<ResponseEntity>
        {
            BuildResponse(22, now.AddMinutes(-1), emptySubmission, q2.Id, a2.Id),
            BuildResponse(11, now.AddMinutes(-2), emptySubmission, q1.Id, a1.Id),
        };

        var submission = EntityBuilders.BuildSubmission(
            id: 55,
            establishmentId: 5,
            sectionId: section.Id,
            responses: responses,
            submissionStatus: SubmissionStatus.InProgress
        );

        var submission2 = EntityBuilders.BuildSubmission(
            id: 55,
            establishmentId: 5,
            sectionId: section.Id,
            submissionStatus: SubmissionStatus.InProgress
        );

        _submissionRepository
            .GetLatestSubmissionAndResponsesAsync(5, section.Id, SubmissionStatus.InProgress)
            .Returns(submission);

        var dto = await sut.GetLatestSubmissionWithOrderedResponsesAsync(
            5,
            section.Id,
            SubmissionStatus.InProgress
        );

        Assert.NotNull(dto);
        Assert.Equal(2, dto!.Responses.Count());
        // Should start at Q1 then go to Q2 per chain
        Assert.Equal(q1q2, dto.Responses.Select(r => r.Question.ContentfulSysId).ToArray());
    }

    [Fact]
    public async Task GetLatestSubmissionWithOrderedResponses_MultipleStatuses_Calls_GetLatestSubmissionAndResponsesAsync_MultipleStatusOverload()
    {
        var sut = CreateServiceUnderTest();
        var section = BuildSection(out _, out _, out _, out _, out _, out _);

        _submissionRepository
            .GetLatestSubmissionAndResponsesAsync(
                1,
                section.Id,
                [SubmissionStatus.Inaccessible, SubmissionStatus.InProgress]
            )
            .Returns((SubmissionEntity?)null);

        var dto = await sut.GetLatestSubmissionWithOrderedResponsesAsync(
            1,
            section.Id,
            [SubmissionStatus.Inaccessible, SubmissionStatus.InProgress]
        );

        await _submissionRepository
            .Received(1)
            .GetLatestSubmissionAndResponsesAsync(
                1,
                section.Id,
                Arg.Is<IEnumerable<SubmissionStatus>>(s =>
                    s.SequenceEqual(
                        new[] { SubmissionStatus.Inaccessible, SubmissionStatus.InProgress }
                    )
                )
            );

        await _submissionRepository
            .DidNotReceive()
            .GetLatestSubmissionAndResponsesAsync(
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<SubmissionStatus?>()
            );
    }

    // ---------- SubmitAnswer ----------
    [Fact]
    public async Task SubmitAnswerAsync_Throws_When_Model_Null()
    {
        var sut = CreateServiceUnderTest();
        await Assert.ThrowsAsync<InvalidDataException>(() => sut.SubmitAnswerAsync(1, 2, 2, null!));
    }

    [Fact]
    public async Task SubmitAnswerAsync_Throws_When_Model_ChosenAnswer_Null()
    {
        var sut = CreateServiceUnderTest();

        var submitAnswer = new SubmitAnswerModel
        {
            SectionId = "S001",
            SectionName = "Test Section 1",
            Question = new IdWithTextModel { Id = "Q900", Text = "Question 900" },
            ChosenAnswer = null,
        };

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.SubmitAnswerAsync(1, 2, 2, submitAnswer)
        );
    }

    [Theory]
    [InlineData("", "Section 1", "Q1", "Question 1", "A1", "Answer 1")]
    [InlineData("SEC001", "", "Q1", "Question 1", "A1", "Answer 1")]
    [InlineData("SEC001", "Section 1", "", "Question 1", "A1", "Answer 1")]
    [InlineData("SEC001", "Section 1", "Q1", "", "A1", "Answer 1")]
    [InlineData("SEC001", "Section 1", "Q1", "Question 1", "", "Answer 1")]
    [InlineData("SEC001", "Section 1", "Q1", "Question 1", "A1", "")]
    public async Task SubmitAnswerAsync_Throws_When_Model_Properties_Empty(
        string sectionId,
        string sectionName,
        string questionId,
        string questionText,
        string answerId,
        string answerText
    )
    {
        var sut = CreateServiceUnderTest();

        var submitAnswer = new SubmitAnswerModel
        {
            SectionId = sectionId,
            SectionName = sectionName,
            Question = new IdWithTextModel { Id = questionId, Text = questionText },
            ChosenAnswer = new IdWithTextModel { Id = answerId, Text = answerText },
        };

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.SubmitAnswerAsync(1, 2, 2, submitAnswer)
        );
    }

    // ---------- GetSectionStatusesAsync ----------
    [Fact]
    public async Task GetSectionStatuses_Joins_Ids_And_Maps_Dtos()
    {
        var sut = CreateServiceUnderTest();
        var entities = new List<SectionStatusEntity>
        {
            new() { SectionId = "S1", Status = SubmissionStatus.CompleteReviewed },
            new() { SectionId = "S2", Status = SubmissionStatus.NotStarted },
        };

        _submissionRepository.GetSectionStatusesAsync("S1,S2", 123).Returns(entities);

        var result = await sut.GetSectionStatusesAsync(123, ["S1", "S2"]);

        Assert.Collection(
            result,
            s =>
            {
                Assert.Equal("S1", s.SectionId);
                Assert.Equal(SubmissionStatus.CompleteReviewed, s.Status);
            },
            s =>
            {
                Assert.Equal("S2", s.SectionId);
                Assert.Equal(SubmissionStatus.NotStarted, s.Status);
            }
        );

        await _submissionRepository.Received(1).GetSectionStatusesAsync("S1,S2", 123);
    }

    // ---------- GetSectionSubmissionStatusAsync ----------
    [Fact]
    public async Task GetSectionSubmissionStatus_When_Found_Completed_True()
    {
        var sut = CreateServiceUnderTest();
        var submission = EntityBuilders.BuildSubmission(
            id: 0,
            establishmentId: 1,
            sectionId: "SEC"
        );

        _submissionRepository
            .GetLatestSubmissionAndResponsesAsync(1, "SEC", SubmissionStatus.CompleteReviewed)
            .Returns(submission);

        var dto = await sut.GetSectionSubmissionStatusAsync(
            1,
            "SEC",
            SubmissionStatus.CompleteReviewed
        );

        Assert.Equal(SubmissionStatus.CompleteReviewed, dto.Status);
    }

    [Fact]
    public async Task GetSectionSubmissionStatus_When_Found_Completed_False()
    {
        var sut = CreateServiceUnderTest();
        var submission = EntityBuilders.BuildSubmission(
            id: 0,
            establishmentId: 1,
            sectionId: "SEC",
            submissionStatus: SubmissionStatus.InProgress
        );

        _submissionRepository
            .GetLatestSubmissionAndResponsesAsync(1, "SEC", SubmissionStatus.InProgress)
            .Returns(submission);

        var dto = await sut.GetSectionSubmissionStatusAsync(1, "SEC", SubmissionStatus.InProgress);

        Assert.Equal(SubmissionStatus.InProgress, dto.Status);
    }

    [Fact]
    public async Task GetSectionSubmissionStatus_When_None_NotStarted()
    {
        var sut = CreateServiceUnderTest();
        _submissionRepository
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
    public async Task SetSubmissionReviewed_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionReviewedAsync(99);
        await _submissionRepository
            .Received(1)
            .SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(99);
    }

    [Fact]
    public async Task SetSubmissionInaccessible_By_Establishment_Section_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionInaccessibleAsync(1, "SEC");
        await _submissionRepository.Received(1).SetSubmissionInaccessibleAsync(1, "SEC");
    }

    [Fact]
    public async Task SetSubmissionInaccessible_By_SubmissionId_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionInaccessibleAsync(123);
        await _submissionRepository.Received(1).SetSubmissionInaccessibleAsync(123);
    }

    [Fact]
    public async Task SetSubmissionInProgress_By_Establishment_Section_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionInProgressAsync(1, "SEC");
        await _submissionRepository.Received(1).SetSubmissionInProgressAsync(1, "SEC");
    }

    [Fact]
    public async Task SetSubmissionInProgress_By_SubmissionId_Delegates()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionInProgressAsync(123);
        await _submissionRepository.Received(1).SetSubmissionInProgressAsync(123);
    }

    [Fact]
    public async Task SetSubmissionDeleted_Delegates_To_SP()
    {
        var sut = CreateServiceUnderTest();
        await sut.SetSubmissionDeletedAsync(1, "SEC");
        await _submissionRepository.Received(1).SetSubmissionDeletedAsync(1, "SEC");
    }

    private void SetupConfirmCheckAnswersAndUpdateRecommendationsAsync(
        out EstablishmentEntity establishment,
        out QuestionnaireSectionEntry section,
        out SubmissionEntity submission,
        out int? matEstablishmentId,
        out int userId
    )
    {
        var establishmentId = Random.Shared.Next();
        matEstablishmentId = Random.Shared.Next();
        userId = Random.Shared.Next();

        establishment = EntityBuilders.BuildEstablishment(establishmentId);
        section = BuildSection(out var _, out var _, out var _, out var _, out var _, out var _);
        submission = EntityBuilders.BuildEmptySubmission();

        _submissionRepository
            .GetSubmissionByIdWithResponsesAsync(submission.Id)
            .Returns(submission);

        _recommendationRepository
            .UpsertRecommendations(Arg.Any<List<SqlRecommendationDto>>())
            .Returns([]);
    }

    // ---------- ConfirmCheckAnswersAndUpdateRecommendationsAsync ----------

    [Fact]
    public async Task ConfirmCheckAnswersAndUpdateRecommendationsAsync_UpsertsRecommendations()
    {
        var sut = CreateServiceUnderTest();
        var section = new QuestionnaireSectionEntry();

        await sut.ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(1, 1, 123, 99, section);

        await _recommendationRepository
            .Received(1)
            .UpsertRecommendations(Arg.Any<List<SqlRecommendationDto>>());
    }

    [Fact]
    public async Task ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync_ThrowsWhenQuestionNotFound()
    {
        // Arrange
        var sut = CreateServiceUnderTest();

        var user = EntityBuilders.BuildUser(101);
        var establishment = EntityBuilders.BuildEstablishment(201);
        var question1 = EntityBuilders.BuildQuestion(301);
        var question2 = EntityBuilders.BuildQuestion(302);
        var answer = EntityBuilders.BuildAnswer(401);
        var submission = EntityBuilders.BuildSubmission(
            501,
            establishment.Id,
            "SEC1",
            submissionStatus: SubmissionStatus.CompleteNotReviewed
        );
        var response = EntityBuilders.BuildResponse(
            601,
            user.Id,
            establishment.Id,
            submission.Id,
            null,
            question1.Id,
            question1.ContentfulRef,
            answer.Id,
            answer.ContentfulRef
        );

        var coreRecommendation = EntryBuilders.BuildRecommendationChunk("R1", "Q999");
        var sectionQuestion = new QuestionnaireQuestionEntry { Sys = new(question2.ContentfulRef) };

        var section = new QuestionnaireSectionEntry
        {
            CoreRecommendations = [coreRecommendation],
            Questions = [sectionQuestion],
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
                establishment.Id,
                null,
                submission.Id,
                user.Id,
                section
            )
        );

        // Assert
        Assert.Equal("Could not find the question identified in the submission", exception.Message);
    }

    [Fact]
    public async Task ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync_UpsertsRecommendations()
    {
        // Arrange
        var sut = CreateServiceUnderTest();

        var user = EntityBuilders.BuildUser(101);
        var establishment = EntityBuilders.BuildEstablishment(201);
        var question = EntityBuilders.BuildQuestion(301);
        var answer = EntityBuilders.BuildAnswer(401);
        var submission = EntityBuilders.BuildSubmission(
            501,
            establishment.Id,
            "SEC1",
            submissionStatus: SubmissionStatus.CompleteNotReviewed
        );
        var response = EntityBuilders.BuildResponse(
            601,
            user.Id,
            establishment.Id,
            submission.Id,
            null,
            question.Id,
            question.ContentfulRef,
            answer.Id,
            answer.ContentfulRef
        );

        var coreRecommendation = EntryBuilders.BuildRecommendationChunk("R1", "Q999");
        var sectionQuestion = new QuestionnaireQuestionEntry { Sys = new(question.ContentfulRef) };

        var section = new QuestionnaireSectionEntry
        {
            CoreRecommendations = [coreRecommendation],
            Questions = [sectionQuestion],
        };

        // Act
        await _recommendationRepository
            .Received(1)
            .UpsertRecommendations(
                Arg.Is<List<SqlRecommendationDto>>(re =>
                    re.First().ContentfulSysId == coreRecommendation.Id
                    && re.First().RecommendationText == coreRecommendation.Header
                    && re.First().QuestionId == question.Id
                    && re.First().QuestionContentfulRef == question.ContentfulRef
                )
            );
    }

    [Fact]
    public async Task ConfirmCheckAnswersAndUpdateRecommendationsAsync_CreatesRecommendationHistories()
    {
        SetupConfirmCheckAnswersAndUpdateRecommendationsAsync(
            out var establishment,
            out var section,
            out var submission,
            out var matEstablishmentId,
            out var userId
        );

        var sut = CreateServiceUnderTest();

        await sut.ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
            establishment.Id,
            null,
            submission.Id,
            userId,
            section
        );

        await _establishmentRecommendationHistoryRepository
            .Received(1)
            .CreateRecommendationHistoriesAsync(
                establishment.Id,
                matEstablishmentId,
                userId,
                Arg.Any<List<RecommendationEntity>>(),
                Arg.Any<Dictionary<string, int>>(),
                Arg.Any<Dictionary<string, RecommendationStatus>>()
            );
    }

    [Fact]
    public async Task ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync_SetsSubmissionStatuses()
    {
        SetupConfirmCheckAnswersAndUpdateRecommendationsAsync(
            out var establishment,
            out var section,
            out var submission,
            out var matEstablishmentId,
            out var userId
        );

        var sut = CreateServiceUnderTest();

        await sut.ConfirmCheckAnswersAndCreateRecommendationHistoriesAsync(
            establishment.Id,
            matEstablishmentId,
            submission.Id,
            userId,
            section
        );

        await _submissionRepository
            .Received(1)
            .SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(
                Arg.Any<int>()
            );
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_CallsRepo()
    {
        var sut = CreateServiceUnderTest();
        var submission = EntityBuilders.BuildSubmission(
            id: 444,
            establishmentId: 1,
            sectionId: "SEC04"
        );

        _submissionRepository.GetSubmissionByIdAsync(submission.Id).Returns(submission);

        await sut.GetSubmissionByIdAsync(submission.Id);

        await _submissionRepository.Received(1).GetSubmissionByIdAsync(submission.Id);
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_ReturnsSubmissionDto()
    {
        var submission = EntityBuilders.BuildSubmission(
            id: 555,
            establishmentId: 1,
            sectionId: "SEC05"
        );

        var sut = CreateServiceUnderTest();

        _submissionRepository.GetSubmissionByIdAsync(submission.Id).Returns(submission);

        var dto = await sut.GetSubmissionByIdAsync(submission.Id);

        Assert.NotNull(dto);
        Assert.Equal(submission.Id, dto.Id);
    }

    [Fact]
    public async Task GetSubmissionByIdAsync_ThrowsWhenNullSubmission()
    {
        var sut = CreateServiceUnderTest();
        SubmissionEntity nullSubmission = null!;
        var submissionId = 666;

        _submissionRepository.GetSubmissionByIdAsync(666).Returns(nullSubmission);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GetSubmissionByIdAsync(submissionId)
        );
        Assert.Equal($"Submission with ID '{submissionId}' not found", ex.Message);
    }
}
