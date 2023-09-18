using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web;

public static class PageRedirecter
{
  private const string PAGES_CONTROLLER = "Pages";
  private const string GET_BY_ROUTE_ACTION = "GetByRoute";
  private const string SELF_ASSESSMENT_ROUTE = "/self-assessment";

  private const string CHECK_ANSWERS_ACTION = "CheckAnswersPage";
  private const string CHECK_ANSWERS_CONTROLLER = "CheckAnswers";

  public static RedirectToActionResult RedirectToSelfAssessment(this Controller controller)
    => controller.RedirectToAction(GET_BY_ROUTE_ACTION, PAGES_CONTROLLER, new { route = SELF_ASSESSMENT_ROUTE });

  public static RedirectToActionResult RedirectToCheckAnswers(this Controller controller, string sectionSlug)
    => controller.RedirectToAction(CHECK_ANSWERS_ACTION, CHECK_ANSWERS_CONTROLLER, new { sectionSlug });
}

