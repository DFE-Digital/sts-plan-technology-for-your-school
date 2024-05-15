namespace Dfe.PlanTech.Application.Constants;

public static class DatabaseConstants
{
    public const string CalculateMaturitySprocParam = "@submissionId";
    public const string CalculateMaturitySproc = $"[dbo].[calculateMaturityForSubmission] {CalculateMaturitySprocParam}";
    public const string GetSectionStatusesForCategory = $"[dbo].[GETSectionStatusesForCategoryKGTest]";
}