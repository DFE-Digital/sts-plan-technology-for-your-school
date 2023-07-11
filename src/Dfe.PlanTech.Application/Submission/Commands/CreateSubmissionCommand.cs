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
        public async Task<int> CreateSubmission(Domain.Submissions.Models.Submission submissionParam)
        {
            var submission = new Domain.Submissions.Models.Submission()
            {
                EstablishmentId = submissionParam.EstablishmentId,
                Completed = false,
                SectionId = submissionParam.SectionId,
                SectionName = submissionParam.SectionName,
                Maturity = submissionParam.Maturity,
                RecomendationId = submissionParam.RecomendationId,
                DateCreated = DateTime.UtcNow,
            };

            _db.AddSubmission(submission);
            await _db.SaveChangesAsync();
            return submission.Id;
        }
    }
}
