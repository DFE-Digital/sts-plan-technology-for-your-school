using Dfe.PlanTech.Domain.Content.Models;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;

public static class GeneratorHelpers
{
    public static void GenerateSys<TEntity>(this BaseGenerator<TEntity> baseGenerator)
    where TEntity : ContentComponent
    {
        baseGenerator.RuleFor(answer => answer.Sys, faker => new SystemDetails() { Id = faker.Random.AlphaNumeric(22) });
    }
}
