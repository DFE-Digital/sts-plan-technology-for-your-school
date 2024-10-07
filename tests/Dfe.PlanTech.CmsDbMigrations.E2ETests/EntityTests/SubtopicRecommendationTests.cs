
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Utilities;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class SubtopicRecommendationTests() : EntityTests<SubtopicRecommendation, SubtopicRecommendationDbEntity, SubtopicRecommendationGenerator>
{
    protected override SubtopicRecommendationGenerator CreateEntityGenerator() => SubtopicRecommendationGenerator.CreateInstance(Db);

    protected override void ClearDatabase()
    {
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationIntroContents]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationSectionAnswers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationSectionChunks]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[SubtopicRecommendationIntros]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationIntros]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[SubtopicRecommendations]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationSections]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationChunkAnswers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationChunks]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Answers]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Titles]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Titles]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Questions]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Sections]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Pages]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[RecommendationIntros]");
        Db.Database.ExecuteSqlRaw("DELETE FROM [Contentful].[Headers]");
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(SubtopicRecommendation entity)
     => new()
     {
         ["intros"] = entity.Intros.ToEntityIds(),
         ["section"] = entity.Section.ToEntityId(),
         ["subtopic"] = entity.Subtopic.ToEntityId(),
     };

    protected override IQueryable<SubtopicRecommendationDbEntity> GetDbEntitiesQuery()
    => Db.SubtopicRecommendations
        .IgnoreAutoIncludes()
        .IgnoreQueryFilters()
        .Include(recSection => recSection.Intros)
        .Include(recSection => recSection.Section)
        .Include(recSection => recSection.Subtopic)
        .AsNoTracking();

    protected override void ValidateDbMatches(SubtopicRecommendation entity, SubtopicRecommendationDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);

        Assert.Equal(entity.Intros.Count, dbEntity.Intros.Count);

        foreach (var content in entity.Intros)
        {
            var matching = dbEntity.Intros.FirstOrDefault(dbIntro => content.Sys.Id == dbIntro.Id);
            Assert.NotNull(matching);
        }

        Assert.Equal(entity.Section.Sys.Id, dbEntity.SectionId);
        Assert.Equal(entity.Subtopic.Sys.Id, dbEntity.SubtopicId);

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}
