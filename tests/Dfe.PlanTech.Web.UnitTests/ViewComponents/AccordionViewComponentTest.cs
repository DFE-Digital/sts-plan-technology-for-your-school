using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents;

public class AccordionViewComponentTest
{
    [Fact]
    public void Accordion_view_component_not_null()
    {
        var accordionComponent = new AccordionViewComponent();

        var result = accordionComponent.Invoke();

        var model = (result as ViewViewComponentResult)?.ViewData?.Model;

        Assert.NotNull(model);
    }
}