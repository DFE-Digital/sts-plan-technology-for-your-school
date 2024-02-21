using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents;

public class AccordionViewComponentTest
{
    [Fact]
    public void Accordion_view_component_not_null()
    {
        IEnumerable<IAccordion> accordionBlock = new List<IAccordion>
            {
                new Accordion() { Order = 1, Title = "Test Header 1", Heading = "Heading 1" },
                new Accordion() { Order = 2, Title = "Test Header 2", Heading = "Heading 2" },
                new Accordion() { Order = 3, Title = "Test Header 3", Heading = "Heading 3" },
                new Accordion() { Order = 4, Title = "Test Header 4", Heading = "Heading 4" }
            };

        var accordionComponent = new AccordionViewComponent();

        var result = accordionComponent.Invoke(accordionBlock);

        var model = (result as ViewViewComponentResult)?.ViewData?.Model;
        
        Assert.NotNull(model);
    }
}