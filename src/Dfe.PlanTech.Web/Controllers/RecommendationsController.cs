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
        public async Task<IActionResult> GetRecommendationPage()
        {
            RecommendationsViewModel recommendationViewModel = new()
            {
                BackUrl = await GetBackUrl()
            };
            
            return View("Recommendations", recommendationViewModel);
        }
    }
}

