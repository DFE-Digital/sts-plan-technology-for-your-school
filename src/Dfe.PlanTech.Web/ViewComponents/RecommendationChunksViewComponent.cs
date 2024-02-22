using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class RecommendationChunksViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IEnumerable<RecommendationChunk> recommendationChunks)
    {
        return View(recommendationChunks);
    }
}