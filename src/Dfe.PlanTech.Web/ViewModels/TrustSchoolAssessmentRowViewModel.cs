using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Web.ViewModels
{
    public class TrustSchoolAssessmentRowViewModel
    {
        public string SchoolName { get; set; } = string.Empty;

        public SubmissionStatus Status { get; set; }

        public string? ViewAnswersHref { get; set; }

        public bool HasAnswersInProgress =>
            Status == SubmissionStatus.InProgress;
    }
}
