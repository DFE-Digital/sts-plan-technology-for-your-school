using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public class WarningComponentGenerator : BaseGenerator<WarningComponent>
{
    protected readonly ReferencedEntityGeneratorHelper<TextBody> TextBodyGeneratorHelper;

    public WarningComponentGenerator(List<TextBody> textBodies)
    {
        TextBodyGeneratorHelper = new(textBodies);
        RuleFor(warningComponent => warningComponent.Text, faker => TextBodyGeneratorHelper.GetEntity(faker));
    }
}
