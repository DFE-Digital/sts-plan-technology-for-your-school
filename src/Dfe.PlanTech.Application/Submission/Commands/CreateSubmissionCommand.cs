using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Submission.Interfaces;
using Dfe.PlanTech.Domain.Answers.Models;
using Dfe.PlanTech.Domain.Submissions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _db.AddSubmission(new Domain.Submissions.Models.Submission() 
            {
                EastablishmentId = submission.EastablishmentId, 
                Completed = false,
                SectionId = submission.SectionId,
                SectionName= submission.SectionName,
                Maturity= submission.Maturity,
                RecomendationId= submission.RecomendationId
            });

            return await _db.SaveChangesAsync();
        }
    }
}
