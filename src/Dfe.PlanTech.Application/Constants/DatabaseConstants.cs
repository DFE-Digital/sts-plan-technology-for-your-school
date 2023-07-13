namespace Dfe.PlanTech.Application.Constants
{
    public class DatabaseConstants
    {
        public const string CalculateMaturitySprocParam = "@submissionId";
        public const string CalculateMaturitySproc = $"[dbo].[calculateMaturityForSubmission] {CalculateMaturitySprocParam}";
    }
}
