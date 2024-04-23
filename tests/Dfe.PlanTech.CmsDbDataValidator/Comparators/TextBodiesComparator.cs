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

    var contentfulRichText = contentfulTextBody["richText"];

    if (contentfulRichText == null)
    {
      Console.WriteLine($"TextBody {contentfulTextBodyId} has no rich text");
      return;
    }

    var errors = RichTextComparator.CompareRichTextContent(matchingDbTextBody.RichText!, contentfulRichText!).ToArray();

    if (errors.Length == 0) return;

    Console.WriteLine($"TextBody {contentfulTextBodyId} has validation errors:\n\n{string.Join("\n ", errors)}");
  }

  /// <remarks>
  /// Due to the size of TextBody entities, fetching all in one will cause timeout issue.
  /// Paginate through them instead.
  /// </remarks>
  protected override async Task<bool> GetDbEntities()
  {
    try
    {
      var entityCount = await GetDbEntitiesQuery().CountAsync();

      var textBodies = new List<ContentComponentDbEntity>(entityCount);

      int skip = 0;
      int take = 50;
      while (true)
      {
        var paginatedTextBodies = await GetDbEntitiesQuery().Skip(skip).Take(take).ToListAsync();
        textBodies.AddRange(paginatedTextBodies);

        Console.WriteLine($"Retrieved {paginatedTextBodies.Count} {_entityType}s from DB. Bringing total to {textBodies.Count} out of {entityCount}");

        skip += take;

        if (skip >= entityCount)
          break;
      }

      _dbEntities = textBodies;

      if (_dbEntities == null || _dbEntities.Count == 0)
      {
        Console.WriteLine($"{_entityType}s not found in database");
        return false;
      }

      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error fetching entities for {_entityType} from DB: {ex.Message}");
      return false;
    }
  }

  protected override IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery()
  {
    return _db.TextBodies;
  }
}