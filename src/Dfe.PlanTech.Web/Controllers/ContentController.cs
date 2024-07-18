using Dfe.ContentSupport.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Controllers
{
    public class ContentController : Controller
    {
        public HomeController _homeController;
        public ContentController(HomeController homeController)
        {
            _homeController = homeController;
        }
        public async Task<IActionResult> Index(string slug)
        {
            return await _homeController.Index(slug);
        }
    }
}
