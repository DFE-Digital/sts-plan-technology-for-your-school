namespace Dfe.PlanTech.Core.Constants;

public static class DatabaseConstants
{
    #region Stored Procedures

    public const string SpCalculateMaturity = "[dbo].[calculateMaturityForSubmission]";
    public const string SpGetSectionStatuses = "[dbo].[GetSectionStatuses]";
    public const string SpSubmitAnswer = "[dbo].[SubmitAnswer]";
    public const string SpSubmitGroupSelection = "[dbo].[SubmitGroupSelection]";

    public const string AnswerContentfulIdParam = "@AnswerContentfulId";
    public const string AnswerTextParam = "@AnswerText";
    public const string EstablishmentIdParam = "@EstablishmentId";
    public const string MaturityParam = "@Maturity";
    public const string QuestionContentfulIdParam = "@QuestionContentfulId";
    public const string QuestionTextParam = "@QuestionText";
    public const string ResponseIdParam = "@ResponseId";
    public const string SectionIdParam = "@SectionId";
    public const string SectionIdsParam = "@SectionIds";
    public const string SectionNameParam = "@SectionName";
    public const string SelectedEstablishmentIdParam = "@SelectedEstablishmentId";
    public const string SelectedEstablishmentNameParam = "@SelectedEstablishmentName";
    public const string SelectionIdParam = "@SelectionId";
    public const string SubmissionIdParam = "@SubmissionId";
    public const string UserIdParam = "@UserId";

    #endregion Stored Procedures
}
