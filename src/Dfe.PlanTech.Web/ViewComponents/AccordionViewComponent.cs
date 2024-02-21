using Dfe.PlanTech.Domain.Content.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class AccordionViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IEnumerable<IAccordion> accordionBlock)
    {
        return View(accordionBlock);
    }
}