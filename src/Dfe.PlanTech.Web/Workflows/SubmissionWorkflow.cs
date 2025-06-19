using Dfe.PlanTech.Infrastructure.Data.Repositories;

namespace Dfe.PlanTech.Web.Workflows
{
    public class SubmissionWorkflow
    {
        private readonly SubmissionRepository _submissionRepository;

        public SubmissionWorkflow(
            SubmissionRepository submissionRepository
        )
        {
            _submissionRepository = submissionRepository;
        }

        public Task DeleteCurrentSubmission(int establishmentId, int sectionId)
        {
            return _submissionRepository.DeleteCurrentSubmission(establishmentId, sectionId);
        }
    }
}
