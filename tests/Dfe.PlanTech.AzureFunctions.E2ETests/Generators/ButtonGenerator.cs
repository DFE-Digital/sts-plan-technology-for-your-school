using Dfe.PlanTech.Domain.Content.Models.Buttons;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class ButtonGenerator : BaseGenerator<Button>
{
  public ButtonGenerator()
  {
    RuleFor(answer => answer.Value, faker => faker.Lorem.Sentence(faker.Random.Int(1, 3)));
    RuleFor(answer => answer.IsStartButton, faker => faker.Random.Bool());
  }
}