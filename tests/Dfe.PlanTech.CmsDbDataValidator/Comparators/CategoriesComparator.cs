
using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Dfe.PlanTech.Infrastructure.Data;

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
        var databaseCategory = TryRetrieveMatchingDbEntity(categories, contentfulCategory);
        if (databaseCategory == null)
        {
            return;
        }

        var headerError = TryGenerateDataValidationError("Header", ValidateChild<CategoryDbEntity>(databaseCategory, "HeaderId", contentfulCategory, "header"));

        var errors = ValidateChildren(contentfulCategory, "sections", databaseCategory, category => category.Sections);

        if (headerError != null)
        {
            errors = errors.Append(headerError);
        }

        ValidateProperties(contentfulCategory, databaseCategory, errors.ToArray());
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
