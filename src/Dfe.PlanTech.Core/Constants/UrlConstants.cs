using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Constants;

[ExcludeFromCodeCoverage]
public static class UrlConstants
{
    #region Routes

    public const string HomePage = "/home";
    public const string Error = "/error";
    public const string NotFound = "/not-found";
    public const string OrgErrorPage = "/dsi-error-not-associated-organisation";
    public const string RecommendationsPage = "/recommendations";
    public const string SelectASchoolPage = "/groups/select-a-school";
    public const string ServerError = "/server-error";

    #endregion Routes

    #region Slugs

    public const string CheckAnswersSlug = "check-answers";
    public const string ViewAnswersSlug = "view-answers";

    public const string GroupsDashboardSlug = "dashboard";
    public const string GroupsSelectionPageSlug = "select-a-school";
    public const string GroupsSlug = "groups";

    public const string Home = "home";

    public const string AutomatedTestingHomePage = "/self-assessment-testing";

    #endregion Slugs
}
