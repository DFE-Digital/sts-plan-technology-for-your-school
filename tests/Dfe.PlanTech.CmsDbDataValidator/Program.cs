using Dfe.PlanTech.CmsDbDataValidator;

ConfigurationSetup.BuildConfiguration();

var db = DatabaseHelpers.CreateDbContext();
var contentfulContent = new ContentfulContent("contentful-export.json");

var comparatorFactory = new ComparatorFactory(db, contentfulContent);

List<string> errors = new(comparatorFactory.Comparators.Count);

foreach (var comparator in comparatorFactory.Comparators)
{
    await comparator.InitialiseContent();
    errors.Add(await comparator.ValidateContentAndPrintErrors());
}

await File.WriteAllTextAsync("contentful-errors.md", string.Join("", errors));