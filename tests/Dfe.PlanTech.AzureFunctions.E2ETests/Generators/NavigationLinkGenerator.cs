using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class NavigationLinkGenerator : BaseGenerator<NavigationLink>
{
    public NavigationLinkGenerator()
    {
        RuleFor(navigationLink => navigationLink.DisplayText, faker => string.Join(",", faker.Lorem.Words(faker.Random.Int(1, 5))));
        RuleFor(navigationLink => navigationLink.Href, faker => faker.Internet.Url());
        RuleFor(navigationLink => navigationLink.OpenInNewTab, faker => faker.Random.Bool());
    }
}