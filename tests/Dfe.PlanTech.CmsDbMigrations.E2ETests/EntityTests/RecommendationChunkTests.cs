
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Generators;
using Dfe.PlanTech.CmsDbMigrations.E2ETests.Utilities;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbMigrations.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class RecommendationChunkTests() : EntityTests<RecommendationChunk, RecommendationChunkDbEntity, RecommendationChunkGenerator>
{
    protected override RecommendationChunkGenerator CreateEntityGenerator() => RecommendationChunkGenerator.CreateInstance(Db);

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(RecommendationChunk entity)
     => new()
     {
         ["header"] = entity.Header,
         ["answers"] = entity.Answers.ToEntityIds(),
         ["content"] = entity.Content.ToEntityIds(),
     };

    protected override IQueryable<RecommendationChunkDbEntity> GetDbEntitiesQuery()
    => Db.RecommendationChunks
        .IgnoreAutoIncludes()
        .IgnoreQueryFilters()
        .Include(recChunk => recChunk.Content)
        .Include(recChunk => recChunk.Answers)
        .AsNoTracking();

    protected override void ValidateDbMatches(RecommendationChunk entity, RecommendationChunkDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);

        Assert.Equal(entity.Header, dbEntity.Header);
        Assert.Equal(entity.Answers.Count, dbEntity.Answers.Count);

        foreach (var answer in entity.Answers)
        {
            var matching = dbEntity.Answers.FirstOrDefault(dbAnswer => answer.Sys.Id == dbAnswer.Id);
            Assert.NotNull(matching);
        }

        Assert.Equal(entity.Content.Count, dbEntity.Content.Count);

        foreach (var content in entity.Content)
        {
            var matching = dbEntity.Content.FirstOrDefault(dbContent => content.Sys.Id == dbContent.Id);
            Assert.NotNull(matching);
        }

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}
