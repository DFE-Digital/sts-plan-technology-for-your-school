using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submissions.Commands;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Models;
using NSubstitute;

namespace Dfe.PlanTech.Web.UnitTests.Submissions
{
    public class MarkSubmissionAsReviewedCommandTests
    {
        private readonly IPlanTechDbContext _db = Substitute.For<IPlanTechDbContext>();
        private readonly MarkSubmissionAsReviewedCommand _command;

        private readonly Submission _submission = new()
        {
            Id = 1,
            SectionId = "section-1",
            SectionName = "Test Section",
            EstablishmentId = 1,
            Status = SubmissionStatus.CompleteNotReviewed.ToString(),
            Completed = true,
        };

        private readonly List<Submission> _otherSubmissions = new()
        {
            new Submission
            {
                Id = 2,
                SectionId = "section-1",
                SectionName = "Test Section",
                EstablishmentId = 1,
                Status = SubmissionStatus.CompleteReviewed.ToString(),
                Deleted = false,
            },
            new Submission
            {
                Id = 3,
                SectionId = "section-1",
                SectionName = "Test Section",
                EstablishmentId = 1,
                Status = SubmissionStatus.CompleteReviewed.ToString(),
                Deleted = false,
            },
        };

        public MarkSubmissionAsReviewedCommandTests()
        {
            _command = new MarkSubmissionAsReviewedCommand(_db);
        }

        [Fact]
        public async Task Should_Mark_Submission_As_Reviewed_And_Update_Timestamp()
        {
            _db.GetSubmissionById(_submission.Id, Arg.Any<CancellationToken>())
                .Returns(_submission);

            _db.GetSubmissions.Returns(
                new AsyncQueryableHelpers.TestAsyncEnumerable<Submission>(_otherSubmissions)
            );

            _db.ToListAsync(Arg.Any<IQueryable<Submission>>(), Arg.Any<CancellationToken>())
                .Returns(_otherSubmissions);

            await _command.MarkSubmissionAsReviewed(_submission.Id, CancellationToken.None);

            Assert.Equal(SubmissionStatus.CompleteReviewed.ToString(), _submission.Status);
            Assert.NotNull(_submission.DateCompleted);
            await _db.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task Should_Invalidate_Other_Reviewed_Submissions()
        {
            _db.GetSubmissionById(_submission.Id, Arg.Any<CancellationToken>())
                .Returns(_submission);

            _db.GetSubmissions.Returns(
                new AsyncQueryableHelpers.TestAsyncEnumerable<Submission>(_otherSubmissions)
            );

            _db.ToListAsync(Arg.Any<IQueryable<Submission>>(), Arg.Any<CancellationToken>())
                .Returns(_otherSubmissions);

            await _command.MarkSubmissionAsReviewed(_submission.Id, CancellationToken.None);

            foreach (var sub in _otherSubmissions)
            {
                Assert.Equal(SubmissionStatus.Inaccessible.ToString(), sub.Status);
                Assert.True(sub.Deleted);
            }
        }

        [Fact]
        public async Task Should_Throw_If_Submission_Not_Found()
        {
            _db.GetSubmissionById(4, Arg.Any<CancellationToken>()).Returns((Submission?)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _command.MarkSubmissionAsReviewed(4, CancellationToken.None)
            );

            Assert.Contains("Submission not found", ex.Message);
        }
    }
}
