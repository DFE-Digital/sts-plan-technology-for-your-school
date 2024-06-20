
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class AnswerTests : EntityTests<Answer, AnswerDbEntity, AnswerGenerator>
{
    protected override AnswerGenerator CreateEntityGenerator() => new();

    protected override void ClearDatabase()
    {
        var dbAnswers = GetDbEntitiesQuery().ToList();

        Db.Answers.RemoveRange(dbAnswers);

        Db.SaveChanges();
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(Answer entity)
     => new()
     {
         ["text"] = entity.Text,
         ["maturity"] = entity.Maturity,
     };

    protected override IQueryable<AnswerDbEntity> GetDbEntitiesQuery()
    => Db.Answers.IgnoreQueryFilters().IgnoreAutoIncludes();

    protected override void ValidateDbMatches(Answer entity, AnswerDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);
        Assert.Equal(entity.Text, dbEntity.Text);
        Assert.Equal(entity.Maturity, dbEntity.Maturity);

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}