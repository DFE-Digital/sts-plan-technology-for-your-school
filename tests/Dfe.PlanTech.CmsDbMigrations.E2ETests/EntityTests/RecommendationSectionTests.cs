
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Utilities;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class RecommendationSectionTests() : EntityTests<RecommendationSection, RecommendationSectionDbEntity, RecommendationSectionGenerator>
{
    protected override RecommendationSectionGenerator CreateEntityGenerator() => RecommendationSectionGenerator.CreateInstance(Db);

    protected override void ClearDatabase()
    {
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationSectionAnswers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationSectionChunks]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationSections]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationChunkAnswers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationChunks]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Answers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Headers]");
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(RecommendationSection entity)
     => new()
     {
         ["answers"] = entity.Answers.ToEntityIds(),
         ["chunks"] = entity.Chunks.ToEntityIds(),
     };

    protected override IQueryable<RecommendationSectionDbEntity> GetDbEntitiesQuery()
    => Db.RecommendationSections
        .IgnoreAutoIncludes()
        .IgnoreQueryFilters()
        .Include(recSection => recSection.Answers)
        .Include(recSection => recSection.Chunks)
        .AsNoTracking();

    protected override void ValidateDbMatches(RecommendationSection entity, RecommendationSectionDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);

        Assert.Equal(entity.Answers.Count, dbEntity.Answers.Count);

        foreach (var answer in entity.Answers)
        {
            var matching = dbEntity.Answers.FirstOrDefault(dbAnswer => answer.Sys.Id == dbAnswer.Id);
            Assert.NotNull(matching);
        }

        Assert.Equal(entity.Chunks.Count, dbEntity.Chunks.Count);

        foreach (var content in entity.Chunks)
        {
            var matching = dbEntity.Chunks.FirstOrDefault(dbContent => content.Sys.Id == dbContent.Id);
            Assert.NotNull(matching);
        }

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}
