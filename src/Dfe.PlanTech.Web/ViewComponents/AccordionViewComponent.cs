using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class AccordionViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IEnumerable<Accordion> accordions)
    {
        return View(accordions.ToList());
    }
}