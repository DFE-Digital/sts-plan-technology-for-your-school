using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class DatabaseConstants
{
    #region Stored Procedures

    public const string SpCalculateMaturity = "[dbo].[CalculateMaturityForSubmission]";
    public const string SpDeleteCurrentSubmission = "[dbo].[DeleteCurrentSubmission]";
    public const string SpGetFirstActivityForEstablishmentRecommendation =
        "[dbo].[GetFirstActivityForEstablishmentRecommendation]";
    public const string SpGetSectionStatuses = "[dbo].[GetSectionStatuses]";
    public const string SpSubmitAnswer = "[dbo].[SubmitAnswer]";

    public const string AnswerContentfulIdParam = "@AnswerContentfulId";
    public const string AnswerTextParam = "@AnswerText";
    public const string EstablishmentIdParam = "@EstablishmentId";
    public const string MaturityParam = "@Maturity";
    public const string QuestionContentfulIdParam = "@QuestionContentfulId";
    public const string QuestionTextParam = "@QuestionText";
    public const string RecommendationContentfulReferenceParam = "@RecommendationContentfulRef";
    public const string ResponseIdParam = "@ResponseId";
    public const string SectionIdParam = "@SectionId";
    public const string SectionIdsParam = "@SectionIds";
    public const string SectionNameParam = "@SectionName";
    public const string SelectedEstablishmentIdParam = "@SelectedEstablishmentId";
    public const string SelectedEstablishmentNameParam = "@SelectedEstablishmentName";
    public const string SelectionIdParam = "@SelectionId";
    public const string SubmissionIdParam = "@SubmissionId";
    public const string UserEstablishmentIdParam = "@UserEstablishmentId";
    public const string UserIdParam = "@UserId";

    #endregion Stored Procedures
}
