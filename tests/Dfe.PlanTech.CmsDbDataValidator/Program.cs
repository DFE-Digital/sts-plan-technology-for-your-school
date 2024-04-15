using Dfe.PlanTech.CmsDbDataValidator;

var configuration = ConfigurationSetup.BuildConfiguration();

var db = DatabaseHelpers.CreateDbContext();
var contentfulContent = new ContentfulContent("contentful-export.json");

var comparatorFactory = new ComparatorFactory(db, contentfulContent);

foreach (var comparator in comparatorFactory.Comparators)
{
  await comparator.InitialiseContent();
  await comparator.ValidateContent();
}