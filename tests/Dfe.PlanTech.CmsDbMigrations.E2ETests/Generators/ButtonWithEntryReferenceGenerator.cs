using Azure;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Content.Models.Buttons;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public class ButtonWithEntryReferenceGenerator : BaseGenerator<ButtonWithEntryReference>
{
    protected readonly ReferencedEntityGeneratorHelper<Button> ButtonGeneratorHelper;
    protected readonly ReferencedEntityGeneratorHelper<Page> PageGeneratorHelper;

    public ButtonWithEntryReferenceGenerator(List<Button> buttons, List<Page> pages)
    {
        ButtonGeneratorHelper = new(buttons);
        PageGeneratorHelper = new(pages);

        RuleFor(b => b.Button, faker => ButtonGeneratorHelper.GetEntity(faker));
        RuleFor(b => b.LinkToEntry, faker => PageGeneratorHelper.GetEntity(faker));
    }
}
