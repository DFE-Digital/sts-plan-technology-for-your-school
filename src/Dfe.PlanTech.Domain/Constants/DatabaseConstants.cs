namespace Dfe.PlanTech.Application.Constants;

public static class DatabaseConstants
{
    public static class StoredProcedures
    {
        public const string CalculateMaturity = $"[dbo].[calculateMaturityForSubmission] {SubmissionIdParam}";
    }

    public const string SubmissionIdParam = "@submissionId";
    public const string GetSectionStatuses = "[dbo].[GetSectionStatuses]";
}
