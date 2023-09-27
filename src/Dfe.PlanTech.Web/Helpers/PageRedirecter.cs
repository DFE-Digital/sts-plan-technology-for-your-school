using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web;

public static class PageRedirecter
{
    public const string PAGES_CONTROLLER = "Pages";
    public const string GET_BY_ROUTE_ACTION = "GetByRoute";
    public const string SELF_ASSESSMENT_ROUTE = "/self-assessment";

    public const string CHECK_ANSWERS_ACTION = "CheckAnswersPage";
    public const string CHECK_ANSWERS_CONTROLLER = "CheckAnswers";

    public static RedirectToActionResult RedirectToSelfAssessment(this Controller controller)
      => controller.RedirectToAction(GET_BY_ROUTE_ACTION, PAGES_CONTROLLER, new { route = SELF_ASSESSMENT_ROUTE });

    public static RedirectToActionResult RedirectToCheckAnswers(this Controller controller, string sectionSlug)
      => controller.RedirectToAction(CHECK_ANSWERS_ACTION, CHECK_ANSWERS_CONTROLLER, new { sectionSlug });
}

