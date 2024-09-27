using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

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

    public static List<Title> GenerateTitles(CmsDbContext db, int count = 2000)
    {
        var titleGenerator = new TitleGenerator();

        var titles = titleGenerator.Generate(count);

        var titleDbEntities = titles.Select(title => new TitleDbEntity()
        {
            Id = title.Sys.Id,
            Published = true,
            Text = title.Text,
        });

        db.Titles.AddRange(titleDbEntities);
        db.SaveChanges();

        return titles;
    }
}
