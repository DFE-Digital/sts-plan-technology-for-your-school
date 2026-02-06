using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.Helpers;

public static class SubmissionHelper
{
    public static SubmissionStatus ToSubmissionStatus(this string submissionStatus)
    {
        if (Enum.TryParse<SubmissionStatus>(submissionStatus, out var status))
        {
            return status;
        }

        throw new ArgumentOutOfRangeException(
            $"Could not parse {submissionStatus} to a known {nameof(SubmissionStatus)}"
        );
    }
}
