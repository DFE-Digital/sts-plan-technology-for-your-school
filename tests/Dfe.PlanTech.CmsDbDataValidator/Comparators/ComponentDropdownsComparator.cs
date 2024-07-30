
using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Models;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class ComponentDropdownsComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, ["Title"], "ComponentDropDown")
{

    protected override async Task<bool> GetDbEntities()
    {
        var dropdownsLoaded = await base.GetDbEntities();

        if (!dropdownsLoaded)
        {
            return false;
        }

        await _db.RichTextContents.Include(content => content.Marks)
                                 .Include(content => content.Data)
                                 .ToListAsync();

        return true;
    }

    public override Task ValidateContent()
    {
        ValidateComponentDropDowns(_dbEntities.OfType<ComponentDropDownDbEntity>().ToArray());
        return Task.CompletedTask;
    }

    private void ValidateComponentDropDowns(ComponentDropDownDbEntity[] componentDropDowns)
    {
        foreach (var contentfulDropdown in _contentfulEntities)
        {
            ValidateComponentDropDown(componentDropDowns, contentfulDropdown);
        }
    }

    private void ValidateComponentDropDown(ComponentDropDownDbEntity[] componentDropDowns, JsonNode contentfulDropdown)
    {
        var databaseDropdown = TryRetrieveMatchingDbEntity(componentDropDowns, contentfulDropdown);

        if (databaseDropdown == null)
        {
            return;
        }

        ValidateProperties(contentfulDropdown, databaseDropdown, GetValidationErrors(databaseDropdown, contentfulDropdown).ToArray());
    }

    protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
    {
        return _db.ComponentDropDowns;
    }

    protected IEnumerable<DataValidationError> GetValidationErrors(ComponentDropDownDbEntity databaseDropdown, JsonNode contentfulDropdown)
    {
        var content = contentfulDropdown["content"];

        if (content == null && databaseDropdown.Content != null)
        {
            yield return GenerateDataValidationError("Content", "Not null in DB but is null in Contentful");
            yield break;
        }

        var errors = RichTextComparator.CompareRichTextContent(databaseDropdown.Content!, content!).ToArray();

        if (errors.Length == 0)
            yield break;

        foreach (var error in errors)
        {
            yield return error;
        }
    }
}
