using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.ViewComponents;

public class AccordionViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IEnumerable<RecommendationChunk> recommendationChunks)
    {

        IEnumerable<Accordion> accordions = recommendationChunks
            .Select((chunk, index) => new Accordion() 
            { 
                Order = index + 1, 
                Title = chunk.Title, 
                Header = chunk.Header.Text
            })
            .ToList();
        
        return View(accordions);
    }
}