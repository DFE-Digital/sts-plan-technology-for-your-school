using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("/privacy")]
    public class PrivacyPolicyController : BaseController<PrivacyPolicyController>
    {
    public PrivacyPolicyController(ILogger<PrivacyPolicyController> logger) : base(logger)
    {
    }

    public async Task<IActionResult> GetPrivacyPage([FromServices] GetPageQuery getPageQuery)
        {
            Page privacyPageContent = await getPageQuery.GetPageBySlug("privacy", CancellationToken.None);

            var privacyViewModel = new PrivacyViewModel()
            {
                Title = privacyPageContent.Title ?? throw new NullReferenceException(nameof(privacyPageContent.Title)),
                Content = privacyPageContent.Content
            };


            return View("Privacy", privacyViewModel);
        }
    }
