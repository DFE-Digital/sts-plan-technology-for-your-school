using System.Diagnostics.CodeAnalysis;
using Dfe.PlanTech.Core.Constants;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Helpers;

[ExcludeFromCodeCoverage]
public static class PageRedirecter
{
    public static RedirectToActionResult RedirectToCategoryLandingPage(this Controller controller, string categorySlug) =>
        controller.RedirectToAction(nameof(PagesController.GetByRoute), nameof(PagesController).GetControllerNameSlug(), new { route = categorySlug });

    public static RedirectToActionResult RedirectToCheckAnswers(this Controller controller, string categorySlug, string sectionSlug, bool? isChangeAnswersFlow) =>
        controller.RedirectToAction(nameof(ReviewAnswersController.CheckAnswers), nameof(ReviewAnswersController).GetControllerNameSlug(), new { categorySlug, sectionSlug, isChangeAnswersFlow });

    public static RedirectToActionResult RedirectToHomePage(this Controller controller) =>
        RedirectToPage(controller, UrlConstants.HomePage);

    public static RedirectToActionResult RedirectToInterstitialPage(this Controller controller, string sectionSlug) =>
        RedirectToPage(controller, sectionSlug);

    public static RedirectToActionResult RedirectToGetQuestionBySlug(this Controller controller, string categorySlug, string sectionSlug, string nextQuestionSlug) =>
        controller.RedirectToAction(nameof(QuestionsController.GetQuestionBySlug), nameof(QuestionsController).GetControllerNameSlug(), new { categorySlug, sectionSlug, nextQuestionSlug });

    public static RedirectToActionResult RedirectToGetNextUnansweredQuestion(this Controller controller, string categorySlug, string sectionSlug) =>
        controller.RedirectToAction(nameof(QuestionsController.GetNextUnansweredQuestion), nameof(QuestionsController).GetControllerNameSlug(), new { categorySlug, sectionSlug });

    private static RedirectToActionResult RedirectToPage(this Controller controller, string route) =>
        controller.RedirectToAction(PagesController.GetPageByRouteAction, nameof(PagesController).GetControllerNameSlug(), new { route });
}
