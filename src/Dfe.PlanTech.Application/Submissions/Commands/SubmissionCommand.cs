using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Submissions.Enums;
using Dfe.PlanTech.Domain.Submissions.Models;

namespace Dfe.PlanTech.Application.Submissions.Commands
{
    public class SubmissionCommand : ISubmissionCommand
    {
        private readonly IPlanTechDbContext _db;

        public SubmissionCommand(IPlanTechDbContext db)
        {
            _db = db;
        }

        public async Task<Submission> CloneSubmission(Submission? existingSubmission, CancellationToken cancellationToken)
        {
            if (existingSubmission is null)
                throw new ArgumentNullException(nameof(existingSubmission), "Submission cannot be null");

            var newSubmission = new Submission
            {
                SectionId = existingSubmission.SectionId,
                SectionName = existingSubmission.SectionName,
                EstablishmentId = existingSubmission.EstablishmentId,
                Completed = false,
                Maturity = existingSubmission.Maturity,
                DateCreated = DateTime.UtcNow,
                Status = SubmissionStatus.InProgress.ToString(),
                Responses = existingSubmission.Responses.Select(r => new Response
                {
                    QuestionId = r.QuestionId,
                    AnswerId = r.AnswerId,
                    UserId = r.UserId,
                    Maturity = r.Maturity,
                    Question = r.Question,
                    Answer = r.Answer,
                    DateCreated = DateTime.UtcNow
                }).ToList()
            };

            _db.AddSubmission(newSubmission);
            await _db.SaveChangesAsync();

            return newSubmission;
        }

        public async Task DeleteSubmission(int submissionId, CancellationToken cancellationToken)
        {
            var submissions = _db.Submissions;
            var submission = submissions.Find(submissionId);

            if (submission == null)
                throw new ArgumentNullException(nameof(submission));

            submission.Status = SubmissionStatus.Inaccessible.ToString();

            await _db.SaveChangesAsync();
        }
    }
}
