using Bogus;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class TextBodyGenerator : BaseGenerator<TextBody>
{
    public RichTextGenerator RichTextGenerator = new();

    public TextBodyGenerator()
    {
        RuleFor(textBody => textBody.RichText, faker => RichTextGenerator.Generate());
    }

    public static IEnumerable<TextBodyDbEntity> MapToDbEntities(IEnumerable<TextBody> textBodies)
    => textBodies.Select(tb => new TextBodyDbEntity()
    {
        RichText = RichTextGenerator.MapToDbEntity(tb.RichText),
    });
}