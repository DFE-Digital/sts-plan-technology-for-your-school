using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Core.Extensions;
using Dfe.PlanTech.Data.Sql.Entities;
using Dfe.PlanTech.Data.Sql.Repositories;

namespace Dfe.PlanTech.Data.Sql.IntegrationTests.Repositories;

public class SubmissionRepositoryTests : DatabaseIntegrationTestBase
{
    private SubmissionRepository _repository = null!;

    public SubmissionRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _repository = new SubmissionRepository(DbContext);
    }

    private AnswerEntity CreateAnswer(int id)
    {
        return new AnswerEntity { ContentfulRef = $"A{id.ToString("000")}", AnswerText = $"Answer {id}" };
    }

    private EstablishmentEntity CreateEstablishment(int id)
    {
        return new EstablishmentEntity { EstablishmentRef = $"EST{id.ToString("000")}", OrgName = $"Test Establishment {id}" };
    }

    private QuestionEntity CreateQuestion(int id)
    {
        return new QuestionEntity { ContentfulRef = $"Q{id.ToString("000")}", QuestionText = $"Question {id}" };
    }

    private QuestionnaireQuestionEntry CreateQuestionEntry(string id)
    {
        return new QuestionnaireQuestionEntry
        {
            Sys = new SystemDetails(id)
        };
    }

    private QuestionnaireAnswerEntry CreateAnswerEntry(string id)
    {
        return new QuestionnaireAnswerEntry
        {
            Sys = new SystemDetails(id)
        };
    }

    private RecommendationChunkEntry CreateRecommendationChunkEntry(
        string id,
        string questionId,
        IEnumerable<string>? completingAnswerIds = null,
        IEnumerable<string>? inProgressAnswerIds = null
    )
    {
        var question = CreateQuestionEntry(questionId);

        var completingAnswers = completingAnswerIds?.Select(CreateAnswerEntry).ToList() ?? [];
        var inProgressAnswers = completingAnswerIds?.Select(CreateAnswerEntry).ToList() ?? [];

        return new RecommendationChunkEntry
        {
            Sys = new SystemDetails(id),
            Header = $"Recommendation {id}",
            Question = question,
            CompletingAnswers = completingAnswers,
            InProgressAnswers = inProgressAnswers
        };
    }

    private ResponseEntity CreateResponse(int id, int userId, int userEstablishmentId, int submissionId, int questionId, int answerId)
    {
        return new ResponseEntity
        {
            UserId = userId,
            UserEstablishmentId = userEstablishmentId,
            SubmissionId = submissionId,
            QuestionId = questionId,
            AnswerId = answerId,
            Maturity = ""
        };
    }

    private SubmissionEntity CreateSubmission(int id, int establishmentId, SubmissionStatus? submissionStatus = SubmissionStatus.CompleteReviewed)
    {
        return new SubmissionEntity
        {
            EstablishmentId = establishmentId,
            Completed = true,
            SectionId = "S001",
            SectionName = "Test Section 1",
            Responses = [],
            DateCreated = DateTime.Now.AddDays(-7),
            DateLastUpdated = DateTime.Now.AddDays(-6),
            DateCompleted = DateTime.Now.AddDays(-5),
            Status = submissionStatus.ToString()
        };
    }

    private UserEntity CreateUser(int id)
    {
        return new UserEntity { DfeSignInRef = $"User{id}" };
    }

    [Fact]
    public async Task SubmissionRepository_CloneSubmission_WhenGivenValidSubmission_ThenCreatesCopyWithNewTimestamps()
    {
        // Arrange
        var user = CreateUser(101);
        var establishment = CreateEstablishment(201);
        var question = CreateQuestion(301);
        var answer = CreateAnswer(401);

        DbContext.Users.Add(user);
        DbContext.Establishments.Add(establishment);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);

        await DbContext.SaveChangesAsync();

        var submission = CreateSubmission(501, establishment.Id, SubmissionStatus.CompleteNotReviewed);
        DbContext.Submissions.Add(submission);

        await DbContext.SaveChangesAsync();

        var response = CreateResponse(601, user.Id, establishment.Id, submission.Id, question.Id, answer.Id);
        DbContext.Responses.Add(response);

        await DbContext.SaveChangesAsync();

        var beforeClone = DateTime.UtcNow;

        // Act
        var clonedSubmission = await _repository.CloneSubmission(submission);

        // Assert
        Assert.NotNull(clonedSubmission);
        Assert.NotEqual(submission.Id, clonedSubmission.Id);
        Assert.Equal(submission.SectionId, clonedSubmission.SectionId);
        Assert.Equal(submission.SectionName, clonedSubmission.SectionName);
        Assert.Equal(submission.EstablishmentId, clonedSubmission.EstablishmentId);
        Assert.Equal(submission.Maturity, clonedSubmission.Maturity);

        // Should have different timestamps and status
        Assert.False(clonedSubmission.Completed);
        Assert.Equal(SubmissionStatus.InProgress.ToString(), clonedSubmission.Status);
        Assert.True(clonedSubmission.DateCreated >= beforeClone);

        // Should clone responses
        Assert.Single(clonedSubmission.Responses);
        var clonedResponse = clonedSubmission.Responses.First();
        Assert.NotEqual(submission.Responses.First().Id, clonedResponse.Id);
        Assert.Equal(submission.Responses.First().QuestionId, clonedResponse.QuestionId);
        Assert.Equal(submission.Responses.First().AnswerId, clonedResponse.AnswerId);
        Assert.True(clonedResponse.DateCreated >= beforeClone);
    }

    [Fact]
    public async Task SubmissionRepository_CloneSubmission_WhenSubmissionIsNull_ThenThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.CloneSubmission(null));
    }

    [Fact]
    public async Task SubmissionRepository_ConfirmCheckAnswersAndUpdateRecommendationsAsync_Throws_When_Submission_Not_Found()
    {
        var section = new QuestionnaireSectionEntry { CoreRecommendations = [] };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _repository.ConfirmCheckAnswersAndUpdateRecommendationsAsync(1, null, 100, 1, section));

        Assert.Equal("Could not find submission with ID 100 in database", exception.Message);
    }

    [Fact]
    public async Task SubmissionRepository_ConfirmCheckAnswersAndUpdateRecommendationsAsync_Throws_When_Question_Not_Found()
    {
        // Arrange
        var user = CreateUser(101);
        var establishment = CreateEstablishment(201);
        var question = new QuestionEntity { ContentfulRef = "Q301ref", QuestionText = "Question 301" };
        var answer = CreateAnswer(401);

        DbContext.Users.Add(user);
        DbContext.Establishments.Add(establishment);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);

        await DbContext.SaveChangesAsync();

        var submission = CreateSubmission(501, establishment.Id, SubmissionStatus.CompleteNotReviewed);
        DbContext.Submissions.Add(submission);

        await DbContext.SaveChangesAsync();

        var response = CreateResponse(601, user.Id, establishment.Id, submission.Id, question.Id, answer.Id);
        DbContext.Responses.Add(response);

        await DbContext.SaveChangesAsync();

        var coreRecommendation = CreateRecommendationChunkEntry("R1", "Q99999");
        var sectionQuestions = new List<QuestionnaireQuestionEntry>
        {
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "Q301ref" } },
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "Q302ref" } }
        };

        var section = new QuestionnaireSectionEntry
        {
            CoreRecommendations = [coreRecommendation],
            Questions = sectionQuestions
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _repository.ConfirmCheckAnswersAndUpdateRecommendationsAsync(establishment.Id, null, submission.Id, user.Id, section));

        // Assert
        Assert.Equal("Could not find the question identified in the submission", exception.Message);
    }

    [Fact]
    public async Task SubmissionRepository_ConfirmCheckAnswersAndUpdateRecommendationsAsync_Generates_SqlRecommendationDto_And_Passes_To_UpsertRecommendations()
    {
        // Arrange
        var user = CreateUser(101);
        var establishment = CreateEstablishment(201);
        var question = new QuestionEntity { ContentfulRef = "Q301ref", QuestionText = "Question 301" };
        var answer = CreateAnswer(401);

        DbContext.Users.Add(user);
        DbContext.Establishments.Add(establishment);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);

        await DbContext.SaveChangesAsync();

        var submission = CreateSubmission(501, establishment.Id, SubmissionStatus.CompleteNotReviewed);
        DbContext.Submissions.Add(submission);

        await DbContext.SaveChangesAsync();

        var response = CreateResponse(601, user.Id, establishment.Id, submission.Id, question.Id, answer.Id);
        DbContext.Responses.Add(response);

        await DbContext.SaveChangesAsync();

        var coreRecommendation = CreateRecommendationChunkEntry("R1", question.ContentfulRef);
        var sectionQuestions = new List<QuestionnaireQuestionEntry>
        {
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "Q301ref" } },
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "Q302ref" } }
        };

        var section = new QuestionnaireSectionEntry
        {
            CoreRecommendations = [coreRecommendation],
            Questions = sectionQuestions
        };

        // Act
        await _repository.ConfirmCheckAnswersAndUpdateRecommendationsAsync(establishment.Id, null, submission.Id, user.Id, section);

        // Assert
        var recommendation = DbContext.Recommendations.FirstOrDefault(r => string.Equals(r.ContentfulRef, coreRecommendation.Id));
        Assert.NotNull(recommendation);
        Assert.Equal(question.Id, recommendation.QuestionId);
        Assert.Equal(coreRecommendation.Header, recommendation.RecommendationText);
    }

    [Fact]
    public async Task SubmissionRepository_ConfirmCheckAnswersAndUpdateRecommendationsAsync_Creates_EstablishmentRecommendationHistories_With_Correct_Recommendation_Statuses()
    {
        // Arrange
        var user = CreateUser(101);
        var establishment = CreateEstablishment(201);
        var question = new QuestionEntity { ContentfulRef = "Q301ref", QuestionText = "Question 301" };
        var answer = CreateAnswer(401);

        DbContext.Users.Add(user);
        DbContext.Establishments.Add(establishment);
        DbContext.Answers.Add(answer);
        DbContext.Questions.Add(question);

        await DbContext.SaveChangesAsync();

        var submission = CreateSubmission(501, establishment.Id, SubmissionStatus.CompleteNotReviewed);
        DbContext.Submissions.Add(submission);

        await DbContext.SaveChangesAsync();

        var response = CreateResponse(601, user.Id, establishment.Id, submission.Id, question.Id, answer.Id);
        DbContext.Responses.Add(response);

        await DbContext.SaveChangesAsync();

        var coreRecommendation = CreateRecommendationChunkEntry("R1", question.ContentfulRef, [answer.ContentfulRef]);
        var sectionQuestions = new List<QuestionnaireQuestionEntry>
        {
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "Q301ref" } },
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "Q302ref" } }
        };

        var section = new QuestionnaireSectionEntry
        {
            CoreRecommendations = [coreRecommendation],
            Questions = sectionQuestions
        };

        // Act
        await _repository.ConfirmCheckAnswersAndUpdateRecommendationsAsync(establishment.Id, null, submission.Id, user.Id, section);

        // Assert
        var recommendation = DbContext.Recommendations.FirstOrDefault(r => string.Equals(r.ContentfulRef, coreRecommendation.Id));
        Assert.NotNull(recommendation);

        var recommendationHistory = DbContext.EstablishmentRecommendationHistories.FirstOrDefault(erh => erh.RecommendationId == recommendation.Id);
        Assert.NotNull(recommendationHistory);
        Assert.Equal(RecommendationStatus.Complete.GetDisplayName(), recommendationHistory.NewStatus);
    }

    [Fact]
    public async Task SubmissionRepository_ConfirmCheckAnswersAndUpdateRecommendationsAsync_Calls_SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync()
    {
        // Arrange
        var user = CreateUser(101);
        var establishment = CreateEstablishment(201);
        var question = new QuestionEntity { ContentfulRef = "Q301ref", QuestionText = "Question 301" };
        var answer = CreateAnswer(401);

        DbContext.Users.Add(user);
        DbContext.Establishments.Add(establishment);
        DbContext.Answers.Add(answer);
        DbContext.Questions.Add(question);

        await DbContext.SaveChangesAsync();

        var oldSubmission = CreateSubmission(501, establishment.Id, SubmissionStatus.CompleteReviewed);
        var newSubmission = CreateSubmission(502, establishment.Id, SubmissionStatus.CompleteNotReviewed);
        DbContext.Submissions.AddRange([oldSubmission, newSubmission]);

        await DbContext.SaveChangesAsync();

        var response = CreateResponse(601, user.Id, establishment.Id, newSubmission.Id, question.Id, answer.Id);
        DbContext.Responses.Add(response);

        await DbContext.SaveChangesAsync();

        var coreRecommendation = CreateRecommendationChunkEntry("R1", question.ContentfulRef, [answer.ContentfulRef]);
        var sectionQuestions = new List<QuestionnaireQuestionEntry>
        {
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "Q301ref" } },
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "Q302ref" } }
        };

        var section = new QuestionnaireSectionEntry
        {
            CoreRecommendations = [coreRecommendation],
            Questions = sectionQuestions
        };

        // Act
        await _repository.ConfirmCheckAnswersAndUpdateRecommendationsAsync(establishment.Id, null, newSubmission.Id, user.Id, section);

        // Assert
        var oldSubmissionEntity = DbContext.Submissions.FirstOrDefault(s => s.Id == oldSubmission.Id);
        var newSubmissionEntity = DbContext.Submissions.FirstOrDefault(s => s.Id == newSubmission.Id);
        Assert.NotNull(oldSubmissionEntity);
        Assert.NotNull(newSubmissionEntity);

        Assert.Equal(SubmissionStatus.Inaccessible.ToString(), oldSubmission.Status);
        Assert.Equal(SubmissionStatus.CompleteReviewed.ToString(), newSubmission.Status);
    }

    [Fact]
    public async Task SubmissionRepository_GetLatestSubmissionAndResponsesAsync_WhenMultipleSubmissionsExist_ThenReturnsMostRecent()
    {
        // Arrange
        var establishment = CreateEstablishment(101);
        var user = CreateUser(201);
        var question1 = CreateQuestion(301);
        var question2 = CreateQuestion(302);
        var answer = CreateAnswer(401);

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.AddRange(question1, question2);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        // Create older submission
        var olderSubmission = new SubmissionEntity
        {
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = establishment.Id,
            Completed = true,
            DateCreated = DateTime.UtcNow.AddDays(-2),
            Status = SubmissionStatus.CompleteNotReviewed.ToString()
        };

        // Create newer submission with multiple responses for same question
        var newerSubmission = new SubmissionEntity
        {
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = establishment.Id,
            Completed = true,
            DateCreated = DateTime.UtcNow.AddDays(-1),
            Status = SubmissionStatus.CompleteNotReviewed.ToString(),
            Responses = new List<ResponseEntity>
            {
                // Older response for Q1
                new ResponseEntity
                {
                    QuestionId = question1.Id,
                    AnswerId = answer.Id,
                    UserId = user.Id,
                    UserEstablishmentId = establishment.Id,
                    Maturity = "Low",
                    DateCreated = DateTime.UtcNow.AddHours(-2)
                },
                // Newer response for Q1 (should be selected)
                new ResponseEntity
                {
                    QuestionId = question1.Id,
                    AnswerId = answer.Id,
                    UserId = user.Id,
                    UserEstablishmentId = establishment.Id,
                    Maturity = "Medium",
                    DateCreated = DateTime.UtcNow.AddHours(-1)
                },
                // Response for Q2
                new ResponseEntity
                {
                    QuestionId = question2.Id,
                    AnswerId = answer.Id,
                    UserId = user.Id,
                    UserEstablishmentId = establishment.Id,
                    Maturity = "High",
                    DateCreated = DateTime.UtcNow.AddHours(-1)
                }
            }
        };

        DbContext.Submissions.AddRange(olderSubmission, newerSubmission);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestSubmissionAndResponsesAsync(establishment.Id, "section-1", true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newerSubmission.Id, result!.Id);

        // Should have only latest response per question
        Assert.Equal(2, result.Responses.Count);
        Assert.Contains(result.Responses, r => r.QuestionId == question1.Id);
        Assert.Contains(result.Responses, r => r.QuestionId == question2.Id);

        // The Q1 response should be the newer one
        var q1Response = result.Responses.First(r => r.QuestionId == question1.Id);
        Assert.True(q1Response.DateCreated > DateTime.UtcNow.AddHours(-1.5));
    }

    [Fact]
    public async Task SubmissionRepository_GetSubmissionByIdAsync_WhenSubmissionExists_ThenReturnsSubmissionWithIncludes()
    {
        // Arrange
        var establishment = CreateEstablishment(101);
        var user = CreateUser(201);
        var question = CreateQuestion(301);
        var answer = CreateAnswer(401);

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        var submission = new SubmissionEntity
        {
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = establishment.Id,
            Status = SubmissionStatus.InProgress.ToString(),
            Responses = new List<ResponseEntity>
            {
                new ResponseEntity
                {
                    QuestionId = question.Id,
                    AnswerId = answer.Id,
                    UserId = user.Id,
                    UserEstablishmentId = establishment.Id,
                    Maturity = "Medium"
                }
            }
        };

        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetSubmissionByIdAsync(submission.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(submission.Id, result!.Id);

        // Verify includes work
        Assert.NotNull(result.Establishment);
        Assert.Equal("Test Establishment 101", result.Establishment.OrgName);

        Assert.Single(result.Responses);
        Assert.NotNull(result.Responses.First().Question);
        Assert.NotNull(result.Responses.First().Answer);
        Assert.Equal("Question 301", result.Responses.First().Question.QuestionText);
        Assert.Equal("Answer 401", result.Responses.First().Answer.AnswerText);
    }

    [Fact]
    public async Task SubmissionRepository_SetLatestSubmissionViewedAsync_WhenSubmissionExists_ThenUpdatesViewedFlag()
    {
        // Arrange
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync();

        var submission = new SubmissionEntity
        {
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = establishment.Id,
            Completed = true,
            Viewed = false,
            DateCreated = DateTime.UtcNow,
            Status = SubmissionStatus.CompleteNotReviewed.ToString()
        };

        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync();

        // Act
        await _repository.SetLatestSubmissionViewedAsync(establishment.Id, "section-1");

        // Assert
        var updated = await DbContext.Submissions.FindAsync(submission.Id);
        Assert.NotNull(updated);
        Assert.True(updated!.Viewed);
    }

    [Fact]
    public async Task SubmissionRepository_SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync_WhenCalled_ThenUpdatesStatuses()
    {
        // Arrange
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        DbContext.Establishments.Add(establishment);
        await DbContext.SaveChangesAsync();

        var submission1 = new SubmissionEntity
        {
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = establishment.Id,
            Status = SubmissionStatus.CompleteReviewed.ToString(),
            DateCreated = DateTime.UtcNow.AddDays(-2)
        };

        var submission2 = new SubmissionEntity
        {
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = establishment.Id,
            Status = SubmissionStatus.CompleteNotReviewed.ToString(),
            DateCreated = DateTime.UtcNow.AddDays(-1)
        };

        DbContext.Submissions.AddRange(submission1, submission2);
        await DbContext.SaveChangesAsync();

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _repository.SetSubmissionReviewedAndOtherCompleteReviewedSubmissionsInaccessibleAsync(submission2.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(SubmissionStatus.CompleteReviewed.ToString(), result.Status);
        Assert.True(result.DateCompleted >= beforeUpdate);

        // Check that the other submission was marked inaccessible
        var otherSubmission = await DbContext.Submissions.FindAsync(submission1.Id);
        Assert.NotNull(otherSubmission);
        Assert.Equal(SubmissionStatus.Inaccessible.ToString(), otherSubmission!.Status);
        Assert.True(otherSubmission.Deleted);
    }


    [Fact]
    public async Task GetQuestionsForSection_ReturnsMatchingQuestions_WhenContentfulRefsMatch()
    {
        // Arrange
        var question1 = new QuestionEntity { ContentfulRef = "ref-101" };
        var question2 = new QuestionEntity { ContentfulRef = "ref-102" };

        DbContext.Questions.AddRange(question1, question2);
        await DbContext.SaveChangesAsync();

        var sectionQuestions = new List<QuestionnaireQuestionEntry>
        {
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "ref-101" } },
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "ref-102" } }
        };
        var section = new QuestionnaireSectionEntry
        {
            Questions = sectionQuestions
        };

        // Act
        var result = await _repository.GetQuestionsForSection(section);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, q => q.ContentfulRef == "ref-101");
        Assert.Contains(result, q => q.ContentfulRef == "ref-102");
    }


    [Fact]
    public async Task GetQuestionsForSection_ReturnsEmptyList_WhenNoMatchingQuestions()
    {
        // Arrange
        var question = new QuestionEntity { ContentfulRef = "ref-201" };

        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var section = new QuestionnaireSectionEntry
        {
            Questions = new List<QuestionnaireQuestionEntry>
            {
                new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "non-existent-ref" } }
            }
        };

        // Act
        var result = await _repository.GetQuestionsForSection(section);

        // Assert
        Assert.Empty(result);
    }


    [Fact]
    public async Task GetQuestionsForSection_IgnoresNullSysIds()
    {
        // Arrange
        var question = new QuestionEntity { ContentfulRef = "ref-301" };

        DbContext.Questions.Add(question);
        await DbContext.SaveChangesAsync();

        var section = new QuestionnaireSectionEntry
        {
            Questions = new List<QuestionnaireQuestionEntry>
        {
            new QuestionnaireQuestionEntry { Sys = null },
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = null! } },
            new QuestionnaireQuestionEntry { Sys = new SystemDetails { Id = "ref-301" } }
        }
        };

        // Act
        var result = await _repository.GetQuestionsForSection(section);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ref-301", result.First().ContentfulRef);
    }

    [Fact]
    public async Task SetSubmissionInProgressAsync_SetsInaccessibleToInProgress()
    {
        // Arrange
        var establishment = CreateEstablishment(101);
        var user = CreateUser(201);
        var question = CreateQuestion(301);
        var answer = CreateAnswer(401);

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        var submission = new SubmissionEntity
        {
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = establishment.Id,
            Status = SubmissionStatus.Inaccessible.ToString(),
            Responses = new List<ResponseEntity>
            {
                new ResponseEntity
                {
                    QuestionId = question.Id,
                    AnswerId = answer.Id,
                    UserId = user.Id,
                    UserEstablishmentId = establishment.Id,
                    Maturity = "Medium"
                }
            }
        };

        DbContext.Submissions.Add(submission);
        await DbContext.SaveChangesAsync();

        var result = await _repository.SetSubmissionInProgressAsync(submission.Id);

        Assert.NotNull(result);
        Assert.Equal(nameof(SubmissionStatus.InProgress), result.Status);
    }
}
