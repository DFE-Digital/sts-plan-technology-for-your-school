using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;
public class RecommendationIntroViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(RecommendationIntro recommendationIntro)
    {
        return View(recommendationIntro);
    }
}