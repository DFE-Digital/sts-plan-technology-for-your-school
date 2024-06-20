using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.Generators;

public class TitleGenerator : BaseGenerator<Title>
{
    public TitleGenerator()
    {
        RuleFor(insetText => insetText.Text, faker => faker.Lorem.Sentences(faker.Random.Int(3, 10)));
    }

    public static IEnumerable<TitleDbEntity> MapToDbEntities(IEnumerable<Title> titles)
     => titles.Select(title => new TitleDbEntity()
     {
         Id = title.Sys.Id,
         Text = title.Text,
         Published = true
     });
}