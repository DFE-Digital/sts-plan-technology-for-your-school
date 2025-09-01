using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewBuilders.Interfaces
{
    public interface IReviewAnswersViewBuilder
    {
        Task<IActionResult> RouteToCheckAnswers(
                    Controller controller,
                    string categorySlug,
                    string sectionSlug,
                    bool? isChangeAnswersFlow,
                    string? errorMessage = null
                );

        Task<IActionResult> RouteToChangeAnswers(
            Controller controller,
            string categorySlug,
            string sectionSlug,
            string? errorMessage = null
        );

        Task<IActionResult> ConfirmCheckAnswers(
            Controller controller,
            string categorySlug,
            string sectionSlug,
            string sectionName,
            int submissionId
        );
    }
}
