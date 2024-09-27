using Dfe.PlanTech.Domain.Content.Enums;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public class HeaderGenerator : BaseGenerator<Header>
{
    public HeaderGenerator()
    {
        RuleFor(cat => cat.Text, faker => faker.Lorem.Sentence(faker.Random.Int(1, 10)));
        RuleFor(cat => cat.Tag, faker => faker.PickRandom<HeaderTag>());
        RuleFor(cat => cat.Size, faker => faker.PickRandom<HeaderSize>());
    }

    public static IEnumerable<HeaderDbEntity> MapToDbEntities(List<Header> headers)
        => headers.Select(header => new HeaderDbEntity()
        {
            Id = header.Sys.Id,
            Tag = header.Tag,
            Text = header.Text,
            Size = header.Size
        });
}
