using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public class PageGenerator : BaseGenerator<Page>
{
    protected readonly ReferencedEntityGeneratorHelper<Title> TitleHelper;
    public PageGenerator(List<Title> titles)
    {
        TitleHelper = new(titles);

        RuleFor(page => page.BeforeTitleContent, (faker, page) =>
        {
            var contentComponents = new List<ContentComponent>();
            var titles = TitleHelper.GetEntities(faker, 0, 5);
            contentComponents.AddRange(titles);
            return contentComponents;
        });
        RuleFor(page => page.Content, (faker, page) =>
        {
            var contentComponents = new List<ContentComponent>();
            var titles = TitleHelper.GetEntities(faker, 0, 5);
            contentComponents.AddRange(titles);
            return contentComponents;
        });

        RuleFor(page => page.DisplayBackButton, faker => faker.Random.Bool());
        RuleFor(page => page.DisplayHomeButton, faker => faker.Random.Bool());
        RuleFor(page => page.DisplayOrganisationName, faker => faker.Random.Bool());
        RuleFor(page => page.DisplayTopicTitle, faker => faker.Random.Bool());
        RuleFor(page => page.InternalName, faker => faker.Lorem.Sentence(1));
        RuleFor(page => page.RequiresAuthorisation, faker => faker.Random.Bool());
        RuleFor(page => page.Slug, faker => faker.Lorem.Slug(faker.Random.Int(1, 8)));
        RuleFor(page => page.Title, faker => TitleHelper.GetEntity(faker));
    }

    public List<Page> GeneratePagesAndSaveToDb(CmsDbContext db, int count)
    {
        var pages = Generate(count);
        var pageDbEntities = MapToDbEntities(pages);
        db.Pages.AddRange(pageDbEntities);
        db.SaveChanges();
        return pages;
    }

    public static IEnumerable<PageDbEntity> MapToDbEntities(IEnumerable<Page> pages)
    => pages.Select(MapToDbEntitiy);

    public static PageDbEntity MapToDbEntitiy(Page page)
    => new()
    {
        Id = page.Sys.Id,
        Slug = page.Slug,
        DisplayBackButton = page.DisplayBackButton,
        DisplayHomeButton = page.DisplayHomeButton,
        DisplayTopicTitle = page.DisplayTopicTitle,
        DisplayOrganisationName = page.DisplayOrganisationName,
        InternalName = page.InternalName,
        RequiresAuthorisation = page.RequiresAuthorisation,
        TitleId = page.Title?.Sys.Id,
        Published = true,
    };

    public static PageGenerator CreateInstance(CmsDbContext db)
    {
        var titles = TitleGenerator.GenerateTitles(db, 2000);

        return new PageGenerator(titles);
    }
}
