using Dfe.PlanTech.Application.Content.Queries;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers;

[Route("/accessibility")]
public class AccessibilityController : BaseController<AccessibilityController>
{
    
    public AccessibilityController(ILogger<AccessibilityController> logger) : base(logger)
    {
        
    }
    public async Task<IActionResult> GetAccessibilityPage([FromServices] GetPageQuery getPageQuery)
    {
        Page accessibilityPageContent = await getPageQuery.GetPageBySlug("accessibility", CancellationToken.None);

        AccessibilityViewModel accessibilityViewModel = new AccessibilityViewModel()
        {
            Title = accessibilityPageContent.Title ?? throw new NullReferenceException(nameof(accessibilityPageContent.Title)),
            Content = accessibilityPageContent.Content,
            UserIsAuthenticated = User.Identity is { IsAuthenticated: true }
        };
        
        return View("Accessibility", accessibilityViewModel);
    }

}