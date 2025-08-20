using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Commands;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Models;
using NSubstitute;
public class SubmissionCommandTests
{
    private readonly IPlanTechDbContext _db = Substitute.For<IPlanTechDbContext>();
    private readonly SubmissionCommand _command;

    public SubmissionCommandTests()
    {
        _command = new SubmissionCommand(_db);
    }

    [Fact]
    public async Task CloneSubmission_Should_Throw_When_Submission_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _command.CloneSubmission(null, CancellationToken.None));
    }

    [Fact]
    public async Task CloneSubmission_Should_Clone_Correctly_And_Save()
    {
        var existingSubmission = new Submission
        {
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = 1,
            Maturity = "Medium",
            Responses = new List<Response>
            {
                new Response
                {
                    QuestionId = 1,
                    AnswerId = 1,
                    UserId = 1,
                    Maturity = "Medium"
                }
            }
        };

        var newSubmission = await _command.CloneSubmission(existingSubmission, CancellationToken.None);

        Assert.NotNull(newSubmission);
        Assert.Equal(existingSubmission.SectionId, newSubmission.SectionId);
        Assert.Equal(existingSubmission.SectionName, newSubmission.SectionName);
        Assert.Equal(existingSubmission.EstablishmentId, newSubmission.EstablishmentId);
        Assert.False(newSubmission.Completed);
        Assert.Equal(SubmissionStatus.InProgress.ToString(), newSubmission.Status);
        Assert.Single(newSubmission.Responses);

        _db.Received().AddSubmission(newSubmission);
        await _db.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteSubmission_Should_Throw_When_Submission_Not_Found()
    {
        _db.Submissions.Find(Arg.Any<int>()).Returns((Submission?)null);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _command.SetSubmissionInaccessible(42, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteSubmission_Should_Mark_Status_And_Save()
    {
        var submission = new Submission { Id = 1, SectionId = "section-1", SectionName = "Test Section", Status = SubmissionStatus.CompleteReviewed.ToString() };
        _db.Submissions.Find(1).Returns(submission);

        await _command.SetSubmissionInaccessible(1, CancellationToken.None);

        Assert.Equal(SubmissionStatus.Inaccessible.ToString(), submission.Status);
        await _db.Received().SaveChangesAsync();
    }
}
