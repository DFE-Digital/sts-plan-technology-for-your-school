using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Tests;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbDataValidator.Comparators;

public class TextBodyComparator(CmsDbContext db, ContentfulContent contentfulContent) : BaseComparator(db, contentfulContent, [], "TextBody")
{
  public override Task ValidateContent()
  {
    ValidateTextBodys(_dbEntities.OfType<TextBodyDbEntity>().ToArray());
    return Task.CompletedTask;
  }

  private void ValidateTextBodys(TextBodyDbEntity[] textBodies)
  {
    foreach (var contentfulTextBody in _contentfulEntities)
    {
      ValidateTextBody(textBodies, contentfulTextBody);
    }
  }

  private void ValidateTextBody(TextBodyDbEntity[] textBodies, JsonNode contentfulTextBody)
  {
    var contentfulTextBodyId = contentfulTextBody.GetEntryId();

    if (contentfulTextBodyId == null)
    {
      Console.WriteLine($"Couldn't find ID for Contentful TextBody {contentfulTextBody}");
      return;
    }

    var matchingDbTextBody = textBodies.FirstOrDefault(TextBody => TextBody.Id == contentfulTextBodyId);

    if (matchingDbTextBody == null)
    {
      Console.WriteLine($"No matching TextBody found for contentful TextBody with ID: {contentfulTextBodyId}");
      return;
    }

    var errors = RichTextComparator.CompareRichTextContent(matchingDbTextBody.RichText, contentfulTextBody).ToArray();

    if (errors.Length == 0) return;

    Console.WriteLine($"TextBody {contentfulTextBodyId} has validation errors: {string.Join("\n ", errors)}");
  }

  protected override IQueryable<TextBodyDbEntity> GetDbEntitiesQuery()
  {
    return _db.TextBodies.Include(tb => tb.RichText).ThenInclude(rt => rt.Marks)
                          .Include(tb => tb.RichText).ThenInclude(rt => rt.Data);
  }
}