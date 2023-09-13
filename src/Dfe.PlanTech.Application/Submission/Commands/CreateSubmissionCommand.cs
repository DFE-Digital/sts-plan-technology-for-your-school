using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;

namespace Dfe.PlanTech.Application.Submission.Commands
{
    public class CreateSubmissionCommand : ICreateSubmissionCommand
    {
        private readonly IPlanTechDbContext _db;

        public CreateSubmissionCommand(IPlanTechDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a new submission in the database
        /// </summary>
        /// <param name="submission"></param>
        /// <returns>
        /// The answer ID
        /// </returns>
        public async Task<int> CreateSubmission(Domain.Submissions.Models.Submission submission)
        {
            var submissionEntity = new Domain.Submissions.Models.Submission()
            {
                EstablishmentId = submission.EstablishmentId,
                Completed = false,
                SectionId = submission.SectionId,
                SectionName = submission.SectionName,
                Maturity = submission.Maturity,
                DateCreated = DateTime.UtcNow,
            };

            _db.AddSubmission(submissionEntity);
            await _db.SaveChangesAsync();
            return submissionEntity.Id;
        }
    }
}
