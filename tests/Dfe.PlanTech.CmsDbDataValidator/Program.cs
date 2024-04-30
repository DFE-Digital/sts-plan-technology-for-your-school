using Dfe.PlanTech.CmsDbDataValidator;

var configuration = ConfigurationSetup.BuildConfiguration();

var db = DatabaseHelpers.CreateDbContext(configuration);

var contentfulContent = new ContentfulContent(configuration);

await contentfulContent.Initialise();

var comparatorFactory = new ComparatorFactory(db, contentfulContent);

List<string> errors = new(comparatorFactory.Comparators.Count);

foreach (var comparator in comparatorFactory.Comparators)
{
    await comparator.InitialiseContent();
    errors.Add(await comparator.ValidateContentAndPrintErrors());
}

await File.WriteAllTextAsync("contentful-errors.md", string.Join("", errors));