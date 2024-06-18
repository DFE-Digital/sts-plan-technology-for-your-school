using Dfe.PlanTech.Domain.Content.Models.Buttons;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class ButtonWithEntryReferenceGenerator : BaseGenerator<ButtonWithEntryReference>
{
  protected readonly ReferencedEntityGeneratorHelper<Button> ButtonGeneratorHelper;

  public ButtonWithEntryReferenceGenerator(List<Button> buttons)
  {
    ButtonGeneratorHelper = new(buttons);

    RuleFor(b => b.Button, faker => ButtonGeneratorHelper.GetEntity(faker));
  }
}