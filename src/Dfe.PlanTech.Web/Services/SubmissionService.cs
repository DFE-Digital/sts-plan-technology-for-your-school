using System.Numerics;
using System.Threading;
using Dfe.PlanTech.Core.Exceptions;
using Dfe.PlanTech.Web.Workflows;
using static System.Collections.Specialized.BitVector32;

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

        /// <summary>
        /// Gets the current status or most recently completed status for the
        /// current user's establishment and the given section
        /// </summary>
        /// <param name="sectionSlug"></param>
        /// <param name="complete"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ContentfulDataUnavailableException"></exception>
        public async Task GetJourneyStatus(int establishmentId, string sectionSlug, bool completed)
        {
            Section = await _getSectionQuery.GetSectionBySlug(sectionSlug, cancellationToken) ?? throw new ContentfulDataUnavailableException($"Could not find section for slug {sectionSlug}");

            SectionStatus = await _getSubmissionStatusesQuery.GetSectionSubmissionStatusAsync(establishmentId,
            Section,
                                                                                              complete,
            cancellationToken);

            var matchingStatusChecker = Array.Find(_statusCheckers, statusChecker => statusChecker.IsMatchingSubmissionStatus(this)) ??
                                        throw new InvalidDataException($"Could not find appropriate status checker for section status {SectionStatus}");

            await matchingStatusChecker.ProcessSubmission(this, cancellationToken);
        }

        public async Task DeleteCurrentSubmission(string sectionId)
        {
            var establishmentId = await _userWorkflow.GetCurrentUserEstablishmentIdAsync();
            await _submissionWorkflow.DeleteCurrentSubmission(establishmentId, sectionId);
        }
    }
}
