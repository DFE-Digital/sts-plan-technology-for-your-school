using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Web.Models;
using Dfe.PlanTech.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Xunit;

namespace Dfe.PlanTech.Web.UnitTests.ViewComponents;

public class HomeLinkViewComponentTests
{
    [Fact]
    public void HomeLinkViewComponentReturnsAuthenticatedLinkWhenUserIsAuthenticated()
    {
        var model = new AccessibilityViewModel
        {
            Title = new Title() { Text = "Accessibility" },
            Content = new ContentComponent[] { new Header() { Tag = Domain.Content.Enums.HeaderTag.H1, Text = "Accessibility" }},
            UserIsAuthenticated = true
        };
        var viewComponent = new HomeLinkViewComponent();

        var result = viewComponent.Invoke(model) as ViewViewComponentResult;

        Assert.NotNull(result);
        Assert.Equal("AuthenticatedLink", result.ViewName);
    }

    [Fact]
    public void HomeLinkViewComponentReturnsUnauthenticatedLinkWhenUserIsNotAuthenticated()
    {
        var model = new AccessibilityViewModel { UserIsAuthenticated = false };
        var viewComponent = new HomeLinkViewComponent();

        var result = viewComponent.Invoke(model) as ViewViewComponentResult;
        
        Assert.NotNull(result);
        Assert.Equal("UnauthenticatedLink", result.ViewName);
    }
}