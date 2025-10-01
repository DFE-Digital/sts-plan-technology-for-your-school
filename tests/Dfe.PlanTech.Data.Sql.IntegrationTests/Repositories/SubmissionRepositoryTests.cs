using Dfe.PlanTech.Core.Enums;
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

    [Fact]
    public async Task SubmissionRepository_CloneSubmission_WhenGivenValidSubmission_ThenCreatesCopyWithNewTimestamps()
    {
        // Arrange
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var answer = new AnswerEntity { AnswerText = "Test Answer", ContentfulRef = "A1" };

        DbContext.Establishments.Add(establishment);
        DbContext.Users.Add(user);
        DbContext.Questions.Add(question);
        DbContext.Answers.Add(answer);
        await DbContext.SaveChangesAsync();

        var originalSubmission = new SubmissionEntity
        {
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = establishment.Id,
            Completed = true,
            Maturity = "High",
            DateCreated = DateTime.UtcNow.AddDays(-1),
            Status = SubmissionStatus.CompleteNotReviewed.ToString(),
            Responses = new List<ResponseEntity>
            {
                new ResponseEntity
                {
                    QuestionId = question.Id,
                    AnswerId = answer.Id,
                    UserId = user.Id,
                    Maturity = "Medium",
                    DateCreated = DateTime.UtcNow.AddDays(-1)
                }
            }
        };

        DbContext.Submissions.Add(originalSubmission);
        await DbContext.SaveChangesAsync();

        var beforeClone = DateTime.UtcNow;

        // Act
        var clonedSubmission = await _repository.CloneSubmission(originalSubmission);

        // Assert
        Assert.NotNull(clonedSubmission);
        Assert.NotEqual(originalSubmission.Id, clonedSubmission.Id);
        Assert.Equal(originalSubmission.SectionId, clonedSubmission.SectionId);
        Assert.Equal(originalSubmission.SectionName, clonedSubmission.SectionName);
        Assert.Equal(originalSubmission.EstablishmentId, clonedSubmission.EstablishmentId);
        Assert.Equal(originalSubmission.Maturity, clonedSubmission.Maturity);

        // Should have different timestamps and status
        Assert.False(clonedSubmission.Completed);
        Assert.Equal(SubmissionStatus.InProgress.ToString(), clonedSubmission.Status);
        Assert.True(clonedSubmission.DateCreated >= beforeClone);

        // Should clone responses
        Assert.Single(clonedSubmission.Responses);
        var clonedResponse = clonedSubmission.Responses.First();
        Assert.NotEqual(originalSubmission.Responses.First().Id, clonedResponse.Id);
        Assert.Equal(originalSubmission.Responses.First().QuestionId, clonedResponse.QuestionId);
        Assert.Equal(originalSubmission.Responses.First().AnswerId, clonedResponse.AnswerId);
        Assert.True(clonedResponse.DateCreated >= beforeClone);
    }

    [Fact]
    public async Task SubmissionRepository_CloneSubmission_WhenSubmissionIsNull_ThenThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.CloneSubmission(null));
    }

    [Fact]
    public async Task SubmissionRepository_GetLatestSubmissionAndResponsesAsync_WhenMultipleSubmissionsExist_ThenReturnsMostRecent()
    {
        // Arrange
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question1 = new QuestionEntity { QuestionText = "Question 1", ContentfulRef = "Q1" };
        var question2 = new QuestionEntity { QuestionText = "Question 2", ContentfulRef = "Q2" };
        var answer = new AnswerEntity { AnswerText = "Answer", ContentfulRef = "A1" };

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
                    Maturity = "Low",
                    DateCreated = DateTime.UtcNow.AddHours(-2)
                },
                // Newer response for Q1 (should be selected)
                new ResponseEntity
                {
                    QuestionId = question1.Id,
                    AnswerId = answer.Id,
                    UserId = user.Id,
                    Maturity = "Medium",
                    DateCreated = DateTime.UtcNow.AddHours(-1)
                },
                // Response for Q2
                new ResponseEntity
                {
                    QuestionId = question2.Id,
                    AnswerId = answer.Id,
                    UserId = user.Id,
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
        var establishment = new EstablishmentEntity { EstablishmentRef = "EST001", OrgName = "Test School" };
        var user = new UserEntity { DfeSignInRef = "user123" };
        var question = new QuestionEntity { QuestionText = "Test Question", ContentfulRef = "Q1" };
        var answer = new AnswerEntity { AnswerText = "Test Answer", ContentfulRef = "A1" };

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
        Assert.Equal("Test School", result.Establishment.OrgName);

        Assert.Single(result.Responses);
        Assert.NotNull(result.Responses.First().Question);
        Assert.NotNull(result.Responses.First().Answer);
        Assert.Equal("Test Question", result.Responses.First().Question.QuestionText);
        Assert.Equal("Test Answer", result.Responses.First().Answer.AnswerText);
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
}
