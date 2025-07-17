using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web;

public static class PageRedirecter
{
    public static RedirectToActionResult RedirectToSelfAssessment(this Controller controller)
    => RedirectToPage(controller, UrlConstants.SelfAssessmentPage);

    public static RedirectToActionResult RedirectToCheckAnswers(this Controller controller, string sectionSlug, bool isChangeAnswersFlow = false)
      => controller.RedirectToAction(CheckAnswersController.CheckAnswersAction, CheckAnswersController.ControllerName, new { sectionSlug, isChangeAnswersFlow });

    public static RedirectToActionResult RedirectToInterstitialPage(this Controller controller, string sectionSlug)
    => RedirectToPage(controller, sectionSlug);

    private static RedirectToActionResult RedirectToPage(this Controller controller, string route)
    => controller.RedirectToAction(PagesController.GetPageByRouteAction, PagesController.ControllerName, new { route });

    public static RedirectToActionResult RedirectToRecommendation(this Controller controller, string sectionSlug, string recommendationSlug)
    => controller.RedirectToAction(RecommendationsController.GetRecommendationAction, RecommendationsController.ControllerName, new { sectionSlug, recommendationSlug });
}

