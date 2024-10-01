using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public class InsetTextGenerator : BaseGenerator<InsetText>
{
    public InsetTextGenerator()
    {
        RuleFor(insetText => insetText.Text, faker => faker.Lorem.Sentences(faker.Random.Int(3, 10)));
    }
}
