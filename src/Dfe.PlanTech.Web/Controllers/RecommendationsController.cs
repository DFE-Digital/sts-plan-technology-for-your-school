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
        public RecommendationsController(ILogger<RecommendationsController> logger) : base(logger) { }

        [HttpGet]
        public Task<IActionResult> GetRecommendationPage()
        {
            RecommendationsViewModel recommendationViewModel = new RecommendationsViewModel()
            {
            };
            return Task.FromResult<IActionResult>(View("Recommendations", recommendationViewModel));
        }
    }
}

