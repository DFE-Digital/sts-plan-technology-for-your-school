using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web;

public static class PageRedirecter
{
    public const string PagesControllerName = nameof(PagesController);
    public const string ReviewAnswersControllerName = nameof(ReviewAnswersController);

    public static RedirectToActionResult RedirectToHomePage(this Controller controller) =>
        RedirectToPage(controller, UrlConstants.HomePage);

    public static RedirectToActionResult RedirectToCheckAnswers(this Controller controller, string categorySlug, string sectionSlug, bool? isChangeAnswersFlow) =>
        controller.RedirectToAction(ReviewAnswersController.CheckAnswersAction, GetControllerName(ReviewAnswersControllerName), new { categorySlug, sectionSlug, isChangeAnswersFlow });

    public static RedirectToActionResult RedirectToInterstitialPage(this Controller controller, string sectionSlug) =>
        RedirectToPage(controller, sectionSlug);

    private static RedirectToActionResult RedirectToPage(this Controller controller, string route) =>
        controller.RedirectToAction(PagesController.GetPageByRouteAction, GetControllerName(PagesControllerName), new { route });

    public static RedirectToActionResult RedirectToCategoryLandingPage(this Controller controller, string categorySlug) =>
        controller.RedirectToAction(PagesController.GetPageByRouteAction, GetControllerName(PagesControllerName), new { route = categorySlug });

    private static string GetControllerName(string controller)
    {
        return controller.Replace("Controller", "");
    }
}
