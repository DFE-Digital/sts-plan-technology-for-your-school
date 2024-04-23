using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Models;
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
    var matchingTextBody = TryRetrieveMatchingDbEntity(textBodies, contentfulTextBody);

    if (matchingTextBody == null)
    {
      return;
    }
    ValidateProperties(contentfulTextBody, matchingTextBody, GenerateDataValidationErrors(matchingTextBody, contentfulTextBody).ToArray());
  }

  protected IEnumerable<DataValidationError> GenerateDataValidationErrors(TextBodyDbEntity dbTextbody, JsonNode contentfulTextBody)
  {
    var contentfulRichText = contentfulTextBody["richText"];

    if (contentfulRichText == null && dbTextbody.RichText != null)
    {
      yield return new DataValidationError("RichText", $"Null in Contentful but exists in DB");
      yield break;
    }
    else if (contentfulRichText != null && dbTextbody.RichText == null)
    {
      yield return new DataValidationError("RichText", $"Null in DB but exists in Contentful");
      yield break;
    }

    var errors = RichTextComparator.CompareRichTextContent(dbTextbody.RichText!, contentfulRichText!).ToArray();

    if (errors.Length == 0) yield break;

    foreach (var error in errors)
    {
      if (error != null) yield return error;
    }
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