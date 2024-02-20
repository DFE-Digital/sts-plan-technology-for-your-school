using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class AccordionViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        //TODO: Replace with actual content.
        IEnumerable<IAccordion> accordionBlock = new List<IAccordion>
        {
            new Accordion() { Order = 1, Header = "Test Header 1", Link = "Link 1" },
            new Accordion() { Order = 2, Header = "Test Header 2", Link = "Link 2" }
        };
        return View(accordionBlock);
    }
}