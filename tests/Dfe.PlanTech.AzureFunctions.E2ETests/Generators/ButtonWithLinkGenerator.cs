using Dfe.PlanTech.Domain.Content.Models.Buttons;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class ButtonWithLinkGenerator : BaseGenerator<ButtonWithLink>
{
  protected readonly ReferencedEntityGeneratorHelper<Button> ButtonGeneratorHelper;

  public ButtonWithLinkGenerator(List<Button> buttons)
  {
    ButtonGeneratorHelper = new(buttons);

    RuleFor(b => b.Button, faker => ButtonGeneratorHelper.GetEntity(faker));
    RuleFor(b => b.Href, faker => faker.Internet.Url());
  }
}
