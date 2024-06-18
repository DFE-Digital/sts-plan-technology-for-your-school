
using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.Plantech.AzureFunctions.E2ETests.EntityTests;

[Collection("ContentComponent")]
public class SectionTests() : EntityTests<Section, SectionDbEntity, SectionGenerator>
{
  protected override SectionGenerator CreateEntityGenerator()
  {
    var questionGenerator = new QuestionGenerator([]);
    var questions = questionGenerator.Generate(400);

    var dbQuestions = QuestionGenerator.MapToDbEntities(questions);
    Db.Questions.AddRange(dbQuestions);
    Db.SaveChanges();

    var pageGenerator = new PageGenerator([]);
    var pages = pageGenerator.Generate(100);

    var pageDbEntities = PageGenerator.MapToDbEntity(pages);
    Db.Pages.AddRange(pageDbEntities);
    Db.SaveChanges();

    return new SectionGenerator(questions, pages);
  }

  protected override void ClearDatabase()
  {
    var questions = Db.Questions.IgnoreQueryFilters().IgnoreAutoIncludes().ToList();
    Db.Questions.RemoveRange(questions);
    Db.SaveChanges();

    var sections = Db.Sections.IgnoreQueryFilters().IgnoreAutoIncludes().ToList();
    Db.Sections.RemoveRange(sections);
    Db.SaveChanges();

    var pages = Db.Pages.IgnoreQueryFilters().IgnoreAutoIncludes().ToList();
    Db.Pages.RemoveRange(pages);
    Db.SaveChanges();
  }

  protected override Dictionary<string, object?> CreateEntityValuesDictionary(Section entity)
   => new()
   {
     ["name"] = entity.Name,
     ["questions"] = entity.Questions.Select(question => new { Sys = new { question.Sys.Id } }),
     ["interstitialPage"] = new
     {
       Sys = new { entity.InterstitialPage.Sys.Id }
     },
   };

  protected override IQueryable<SectionDbEntity> GetDbEntitiesQuery() => Db.Sections.IgnoreQueryFilters().IgnoreAutoIncludes().Select(section => new SectionDbEntity()
  {
    Id = section.Id,
    Name = section.Name,
    Published = section.Published,
    Archived = section.Archived,
    Deleted = section.Deleted,
    Questions = section.Questions.Select(question => new QuestionDbEntity()
    {
      Id = question.Id
    }).ToList()
  });

  protected override void ValidateDbMatches(Section entity, SectionDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
  {
    Assert.NotNull(dbEntity);

    Assert.Equal(entity.Name, dbEntity.Name);

    ValidateEntityState(dbEntity, published, archived, deleted);

    var sectionQuestions = Db.Questions.Where(question => question.SectionId == entity.Sys.Id).ToList();

    Assert.Equal(entity.Questions.Count, sectionQuestions.Count);

    foreach (var question in entity.Questions)
    {
      var matchingQuestion = sectionQuestions.FirstOrDefault(dbQuestion => dbQuestion.Id == question.Sys.Id);
      Assert.NotNull(matchingQuestion);
    }
  }
}