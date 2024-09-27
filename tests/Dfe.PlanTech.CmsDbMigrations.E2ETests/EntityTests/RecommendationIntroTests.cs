
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Utilities;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class RecommendationIntroTests() : EntityTests<RecommendationIntro, RecommendationIntroDbEntity, RecommendationIntroGenerator>
{
    protected override RecommendationIntroGenerator CreateEntityGenerator() => RecommendationIntroGenerator.CreateInstance(Db);

    protected override void ClearDatabase()
    {
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationIntroContents]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Titles]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Answers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationIntros]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Headers]");
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(RecommendationIntro entity)
     => new()
     {
         ["content"] = entity.Content.ToEntityIds(),
         ["header"] = entity.Header.ToEntityId(),
         ["maturity"] = entity.Maturity,
         ["slug"] = entity.Slug,
     };

    protected override IQueryable<RecommendationIntroDbEntity> GetDbEntitiesQuery()
    => Db.RecommendationIntros
        .IgnoreAutoIncludes()
        .IgnoreQueryFilters()
        .Include(recIntro => recIntro.Content)
        .Include(recIntro => recIntro.Header)
        .AsNoTracking();

    protected override void ValidateDbMatches(RecommendationIntro entity, RecommendationIntroDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);

        Assert.Equal(entity.Maturity, dbEntity.Maturity);
        Assert.Equal(entity.Slug, dbEntity.Slug);

        Assert.Equal(entity.Header.Sys.Id, dbEntity.HeaderId);

        Assert.Equal(entity.Content.Count, dbEntity.Content.Count);

        foreach (var content in entity.Content)
        {
            var matching = dbEntity.Content.FirstOrDefault(dbContent => content.Sys.Id == dbContent.Id);
            Assert.NotNull(matching);
        }

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}
