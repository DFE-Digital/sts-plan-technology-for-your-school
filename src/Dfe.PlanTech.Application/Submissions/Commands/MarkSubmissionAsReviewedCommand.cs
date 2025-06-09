using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Interfaces;

namespace Dfe.PlanTech.Application.Submissions.Commands
{
    public class MarkSubmissionAsReviewedCommand : IMarkSubmissionAsReviewedCommand
    {
        private readonly IPlanTechDbContext _db;

        public MarkSubmissionAsReviewedCommand(IPlanTechDbContext db)
        {
            _db = db;
        }

        public async Task MarkSubmissionAsReviewed(int submissionId, CancellationToken cancellationToken)
        {
            var submission = await _db.GetSubmissionById(submissionId, cancellationToken)
                             ?? throw new InvalidOperationException($"Submission not found for id {submissionId}");

            submission.Status = SubmissionStatus.CompleteReviewed.ToString();
            submission.DateCompleted = DateTime.UtcNow;

            var otherSubmissions = await _db.ToListAsync(
                _db.GetSubmissions.Where(s =>
                    s.SectionId == submission.SectionId &&
                    s.EstablishmentId == submission.EstablishmentId &&
                    s.Status == SubmissionStatus.CompleteReviewed.ToString() &&
                    s.Id != submission.Id),
                cancellationToken
            );

            foreach (var oldSubmissions in otherSubmissions)
            {
                oldSubmissions.Status = SubmissionStatus.Inaccessible.ToString();
            }

            await _db.SaveChangesAsync();
        }
    }
}
