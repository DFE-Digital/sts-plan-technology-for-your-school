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
            new Accordion() { Order = 1, Header = "Test Header 1", Link = "broadband-connection" },
            new Accordion() { Order = 2, Header = "Test Header 2", Link = "broadband-connection" },
            new Accordion() { Order = 3, Header = "Test Header 3", Link = "broadband-connection" },
            new Accordion() { Order = 4, Header = "Test Header 4", Link = "broadband-connection" }
        };
        return View(accordionBlock);
    }
}