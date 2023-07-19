using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers
{
    [Authorize]
    [Route("/recommendations")]
    public class RecommendationsController : BaseController<RecommendationsController>
    {
        public RecommendationsController(ILogger<RecommendationsController> logger, IUrlHistory history) : base(logger, history) { }

        [HttpGet]
        public IActionResult GetRecommendationPage()
        {
            RecommendationsViewModel recommendationViewModel = new RecommendationsViewModel()
            {
                BackUrl = history.LastVisitedUrl?.ToString() ?? "self-assessment",
            };
            return View("Recommendations", recommendationViewModel);
        }
    }
}

