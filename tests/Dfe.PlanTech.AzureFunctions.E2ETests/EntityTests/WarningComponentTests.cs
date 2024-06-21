
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class WarningComponentTests() : EntityTests<WarningComponent, WarningComponentDbEntity, WarningComponentGenerator>
{
    protected override WarningComponentGenerator CreateEntityGenerator()
    {
        var textBodyGenerator = new TextBodyGenerator();
        var textBodies = textBodyGenerator.Generate(2000);

        Db.TextBodies.AddRange(TextBodyGenerator.MapToDbEntities(textBodies));
        Db.SaveChanges();

        return new WarningComponentGenerator(textBodies);
    }

    protected override void ClearDatabase()
    {
        var textBodys = Db.TextBodies.IgnoreAutoIncludes().IgnoreQueryFilters().ToList();
        Db.TextBodies.RemoveRange(textBodys);
        Db.SaveChanges();

        var questions = GetDbEntitiesQuery().ToList();
        Db.Warnings.RemoveRange(questions);
        Db.SaveChanges();
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(WarningComponent entity)
     => new()
     {
         ["text"] = new { Sys = new { entity.Text.Sys.Id } }
     };

    protected override IQueryable<WarningComponentDbEntity> GetDbEntitiesQuery()
    => Db.Warnings.IgnoreQueryFilters().AsNoTracking();

    protected override void ValidateDbMatches(WarningComponent entity, WarningComponentDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);

        TextBodyTests.ValidateRichTextContent(entity.Text.RichText, dbEntity.Text.RichText);

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}