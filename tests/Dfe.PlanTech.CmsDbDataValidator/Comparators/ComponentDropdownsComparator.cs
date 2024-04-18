
using System.Text.Json.Nodes;
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
    var databaseDropdown = ValidateChildEntityExistsInDb(componentDropDowns, contentfulDropdown);
    if (databaseDropdown == null)
    {
      return;
    }


    var content = contentfulDropdown["content"];

    if (content == null && databaseDropdown.Content != null)
    {
      Console.WriteLine($"Content for {databaseDropdown.Id} is not null, but is null in Contentful");
      return;
    }

    var errors = RichTextComparator.CompareRichTextContent(databaseDropdown.Content!, content!).ToArray();

    if (errors.Length == 0) return;

    Console.WriteLine($"TextBody {contentfulDropdown} has validation errors: {string.Join("\n ", errors)}");
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.ComponentDropDowns;
  }
}