using Dfe.PlanTech.Web.Workflows;

namespace Dfe.PlanTech.Web.Services
{
    public class SubmissionService
    {
        private readonly SubmissionWorkflow _submissionWorkflow;
        private readonly UserWorkflow _userWorkflow;

        public SubmissionService(
            SubmissionWorkflow submissionWorkflow,
            UserWorkflow userWorkflow
        )
        {
            _submissionWorkflow = submissionWorkflow;
            _userWorkflow = userWorkflow;
        }

        public async Task DeleteCurrentSubmission(int sectionId)
        {
            var establishmentId = await _userWorkflow.GetCurrentUserEstablishmentIdAsync();
            await _submissionWorkflow.DeleteCurrentSubmission(establishmentId, sectionId);
        }
    }
}
