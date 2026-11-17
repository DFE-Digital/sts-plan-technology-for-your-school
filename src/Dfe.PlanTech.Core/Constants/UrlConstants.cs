using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class UrlConstants
{
    #region Routes

    public const string HomePage = "/home";
    public const string Error = "/error";
    public const string NotFound = "/not-found";
    public const string NotifyError = "/email-sharing-error";
    public const string OrgErrorPage = "/dsi-error-not-associated-organisation";
    public const string RecommendationsPage = "/recommendations";
    public const string SelectASchoolPage = "/trust/select-a-school";
    public const string GroupStandards = "/trust/standards";
    public const string ServerError = "/server-error";

    #endregion Routes

    #region Slugs

    public const string CheckAnswersSlug = "check-answers";
    public const string ViewAnswersSlug = "view-answers";

    public const string GroupsSelectionPageSlug = "select-a-school";
    public const string GroupSelfAssessmentSelectionSlug = "select-a-self-assessment";
    public const string TrustSlug = "trust";
    public const string GroupsSelectSchoolsToAssessSlug = "select-schools";
    public const string GroupsSelectSchoolsToUpdateStatusSlug = "select-schools-to-update-status";
    public const string GroupsSelectStatusToUpdateSlug = "select-status";
    public const string GroupsUpdateSchoolsStatus = "update-schools-status";


    public const string Home = "home";

    public const string AutomatedTestingHomePage = "/self-assessment-testing";

    #endregion Slugs
}
