using Bogus;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

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
        Id = tb.Sys.Id,
        RichText = RichTextGenerator.MapToDbEntity(tb.RichText),
    });
}
