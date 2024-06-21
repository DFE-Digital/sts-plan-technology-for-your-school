using Dfe.PlanTech.AzureFunctions.E2ETests.Generators;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.AzureFunctions.E2ETests.EntityTests;

public class CategoryTests : EntityTests<Category, CategoryDbEntity, CategoryGenerator>
{
    protected override void ClearDatabase()
    {
        var pageContents = Db.PageContents.IgnoreAutoIncludes().IgnoreQueryFilters().ToList();
        Db.PageContents.RemoveRange(pageContents);
        Db.SaveChanges();

        var questions = Db.Questions.IgnoreAutoIncludes().IgnoreQueryFilters().ToList();
        Db.Questions.RemoveRange(questions);
        Db.SaveChanges();

        var sections = Db.Sections.IgnoreAutoIncludes().IgnoreQueryFilters().ToList();
        Db.Sections.RemoveRange(sections);
        Db.SaveChanges();

        var categories = Db.Categories.IgnoreAutoIncludes().IgnoreQueryFilters().ToList();
        Db.Categories.RemoveRange(categories);
        Db.SaveChanges();

        var headers = Db.Headers.IgnoreAutoIncludes().IgnoreQueryFilters().ToList();
        Db.Headers.RemoveRange(headers);
        Db.SaveChanges();

        var pages = Db.Pages.IgnoreQueryFilters().IgnoreAutoIncludes().ToList();
        Db.Pages.RemoveRange(pages);
        Db.SaveChanges();

        Db.ChangeTracker.Clear();
    }

    protected override CategoryGenerator CreateEntityGenerator()
    {
        var headerGenerator = new HeaderGenerator();
        var headers = headerGenerator.Generate(100);
        var headerDbEntities = headers.Select(header => new HeaderDbEntity()
        {
            Id = header.Sys.Id,
            Tag = header.Tag,
            Size = header.Size,
            Text = header.Text
        });

        Db.Headers.AddRange(headerDbEntities);
        Db.SaveChanges();

        var pageGenerator = PageGenerator.CreateInstance(Db);
        var pages = pageGenerator.GeneratePagesAndSaveToDb(Db, 250);

        var questionGenerator = QuestionGenerator.CreateInstance(Db);
        var questions = questionGenerator.GenerateQuestionsAndSaveToDb(Db, 500);

        var sectionGenerator = new SectionGenerator(questions, pages);
        var sections = sectionGenerator.Generate(250);
        var sectionDbEntities = sections.Select(section => new SectionDbEntity()
        {
            Id = section.Sys.Id,
            InterstitialPageId = section.InterstitialPage.Sys.Id,
            Name = section.Name,
            Questions = [],
        });

        Db.Sections.AddRange(sectionDbEntities);
        Db.SaveChanges();

        return new CategoryGenerator(headers, sections);
    }

    protected override Dictionary<string, object?> CreateEntityValuesDictionary(Category entity)
    => new()
    {
        ["internalName"] = entity.InternalName,
        ["header"] = new { Sys = new { entity.Header.Sys.Id } },
        ["sections"] = entity.Sections.Select(section => new { Sys = new { section.Sys.Id } }).ToList(),
    };

    protected override IQueryable<CategoryDbEntity> GetDbEntitiesQuery()
      => Db.Categories.IgnoreAutoIncludes().IgnoreQueryFilters();

    protected override void ValidateDbMatches(Category entity, CategoryDbEntity? dbEntity, bool published = true, bool archived = false, bool deleted = false)
    {
        Assert.NotNull(dbEntity);

        Assert.Equal(entity.Header.Sys.Id, dbEntity.HeaderId);

        var sections = Db.Sections.Where(section => section.CategoryId == entity.Sys.Id).ToList();

        Assert.Equal(entity.Sections.Count, sections.Count);

        foreach (var section in sections)
        {
            var matching = sections.FirstOrDefault(dbSection => dbSection.Id == section.Id);
            Assert.NotNull(matching);
        }

        ValidateEntityState(dbEntity, published, archived, deleted);
    }
}