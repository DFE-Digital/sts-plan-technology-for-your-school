
using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class CategoriesComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["InternalName"], "Category")
{

  public override Task ValidateContent()
  {
    ValidateCategories(_dbEntities.OfType<CategoryDbEntity>().ToArray());
    return Task.CompletedTask;
  }

  private void ValidateCategories(CategoryDbEntity[] categories)
  {
    foreach (var category in _contentfulEntities)
    {
      ValidateCategory(categories, category);
    }
  }

  private void ValidateCategory(CategoryDbEntity[] categories, JsonNode contentfulCategory)
  {
    var databaseCategory = ValidateChildEntityExistsInDb(categories, contentfulCategory);
    if (databaseCategory == null)
    {
      return;
    }

    ValidateProperties(contentfulCategory, databaseCategory);

    var headerValidationResult = ValidateReferences<CategoryDbEntity>(databaseCategory, "HeaderId", contentfulCategory, "header");
    if (headerValidationResult != null)
    {
      Console.WriteLine(headerValidationResult);
    }

    ValidateChildren(contentfulCategory, "sections", databaseCategory, category => category.Sections);
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  => _db.Categories.Select(category => new CategoryDbEntity()
  {
    InternalName = category.InternalName,
    Id = category.Id,
    HeaderId = category.HeaderId,
    Sections = category.Sections.Select(section => new SectionDbEntity()
    {
      Id = section.Id
    }).ToList()
  });
}