using Dfe.PlanTech.Application.Constants;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web;

public static class PageRedirecter
{
    public const string SelfAssessmentRoute = UrlConstants.SelfAssessmentPage;

    public static RedirectToActionResult RedirectToSelfAssessment(this Controller controller)
    => RedirectToPage(controller, SelfAssessmentRoute);

    public static RedirectToActionResult RedirectToCheckAnswers(this Controller controller, string sectionSlug)
      => controller.RedirectToAction(CheckAnswersController.CheckAnswersAction, CheckAnswersController.ControllerName, new { sectionSlug });

    public static RedirectToActionResult RedirectToInterstitialPage(this Controller controller, string sectionSlug)
    => RedirectToPage(controller, sectionSlug);

    private static RedirectToActionResult RedirectToPage(this Controller controller, string route)
    => controller.RedirectToAction(PagesController.GetPageByRouteAction, PagesController.ControllerName, new { route });
}

