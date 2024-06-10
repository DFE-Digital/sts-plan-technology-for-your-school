using Bogus;
using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.AzureFunctions.E2ETests;

public static class ContentComponentGeneratorHelpers
{
  public static void AddCommonFakes<TEntity>(this Faker<TEntity> faker)
  where TEntity : ContentComponentDbEntity
  {
    faker.RuleFor(cc => cc.Id, faker => faker.Random.AlphaNumeric(22));
    faker.RuleFor(a => a.Archived, false);
    faker.RuleFor(a => a.Deleted, false);
    faker.RuleFor(a => a.Published, true);
  }
}