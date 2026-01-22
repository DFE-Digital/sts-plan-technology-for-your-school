using Dfe.PlanTech.Web.ViewModels.Inputs;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface IQuestionsViewBuilder
    {
        Task<IActionResult> RouteBySlugAndQuestionAsync(
            Controller controller,
            string categorySlug,
            string sectionSlug,
            string questionSlug,
            string? returnTo
        );
        Task<IActionResult> RouteToInterstitialPage(
            Controller controller,
            string categorySlug,
            string sectionSlug
        );
        Task<IActionResult> RouteByQuestionId(Controller controller, string questionId);
        Task<IActionResult> RouteToNextUnansweredQuestion(
            Controller controller,
            string categorySlug,
            string sectionSlug
        );
        Task<IActionResult> RouteToContinueSelfAssessmentPage(
            Controller controller,
            string categorySlug,
            string sectionSlug
        );
        Task<IActionResult> RestartSelfAssessment(
            Controller controller,
            string categorySlug,
            string sectionSlug,
            bool isObsoleteSubmissionFlow
        );
        Task<IActionResult> ContinuePreviousAssessment(
            Controller controller,
            string categorySlug,
            string sectionSlug
        );
        Task<IActionResult> SubmitAnswerAndRedirect(
            Controller controller,
            SubmitAnswerInputViewModel answerViewModel,
            string categorySlug,
            string sectionSlug,
            string questionSlug,
            string? returnTo
        );
    }
}
