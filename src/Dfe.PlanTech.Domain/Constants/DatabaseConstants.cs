namespace Dfe.PlanTech.Application.Constants;

public static class DatabaseConstants
{
    #region Stored Procedures

    public const string SpCalculateMaturity = "[dbo].[calculateMaturityForSubmission]";
    public const string SpGetSectionStatuses = "[dbo].[GetSectionStatuses]";
    public const string SpSubmitGroupSelection = "[dbo].[SubmitGroupSelection]";

    public const string EstablishmentIdParam = "@EstablishmentId";
    public const string SectionIdsParam = "@SectionIds";
    public const string SelectedEstablishmentIdParam = "@SelectedEstablishmentId";
    public const string SelectedEstablishmentNameParam = "@SelectedEstablishmentName";
    public const string SelectionIdParam = "@SelectionId";
    public const string SubmissionIdParam = "@submissionId";
    public const string UserIdParam = "@UserId";

    #endregion Stored Procedures
}
