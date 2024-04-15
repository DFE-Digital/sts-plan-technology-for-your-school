using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbDataValidator;

public class ContentfulDatabaseComparator<TDbEntity>
where TDbEntity : ContentComponentDbEntity
{
  private List<TDbEntity>? _dbEntities;
  private List<JsonNode>? _contentfulEntities;

  public ContentfulDatabaseComparator()
  {
  }

  public async Task LoadDbEntities(IQueryable<TDbEntity> queryable)
  {
    _dbEntities = await queryable.ToListAsync();
    if (_dbEntities == null || _dbEntities.Count == 0)
    {
      Console.WriteLine("No entities found in the database.");
    }
  }

  public void LoadContentfulEntities(ContentfulContent content, string contentType)
  {
    _contentfulEntities = content.GetEntriesForContentType(contentType).ToList();
    if (_contentfulEntities == null || _contentfulEntities.Count == 0)
    {
      Console.WriteLine("No entities found in Contentful.");
    }
  }

  public void CompareContent(Action<JsonNode, TDbEntity> contentMatchesPredicate)
  {
    if (_contentfulEntities == null || _dbEntities == null || _contentfulEntities.Count == 0 || _dbEntities.Count == 0)
    {
      Console.WriteLine("No entities to compare.");
      return;
    }

    foreach (var contentfulEntry in _contentfulEntities)
    {
      var matchingDbEntry = _dbEntities.FirstOrDefault(dbEntry => dbEntry.Id == contentfulEntry.GetEntryId());

      if (matchingDbEntry == null)
      {
        Console.WriteLine($"No matching entry found for contentful entry with ID: {contentfulEntry.GetEntryId()}");
        continue;
      }

      contentMatchesPredicate(contentfulEntry, matchingDbEntry);
    }
  }

  /// <summary>
  /// Validates array references for a given contentful entry and db entry.
  /// </summary>
  /// <typeparam name="TDbEntityReferences">The type of the db entity references.</typeparam>
  /// <param name="contentfulEntry">The contentful entry to validate.</param>
  /// <param name="arrayKey">The key of the array to validate.</param>
  /// <param name="dbEntry">The db entry to use for validation.</param>
  /// <param name="selectReferences">A function to select references from the db entry.</param>
  public void ValidateArrayReferences<TDbEntityReferences>(JsonNode contentfulEntry, string arrayKey, TDbEntity dbEntry, Func<TDbEntity, List<TDbEntityReferences>> selectReferences)
      where TDbEntityReferences : ContentComponentDbEntity
  {
    var references = contentfulEntry[arrayKey]?.AsArray();

    if (references == null)
    {
      return;
    }

    foreach (var reference in references)
    {
      var referenceId = reference?["sys"]?["id"]?.GetValue<string>();
      var matchingReference = selectReferences(dbEntry).Find(entry => entry.Id == referenceId);

      if (matchingReference == null)
      {
        Console.WriteLine("Error: Matching reference not found.");
      }
    }
  }

  public void ValidateEnumValue<TEnum>(JsonNode contentfulEntry, string key, TDbEntity dbEntry, TEnum dbValue)
      where TEnum : struct, Enum
  {
    var enumValue = GetEnumValue<TEnum>(contentfulEntry, key, dbEntry);

    if (!enumValue.Equals(dbValue))
    {
      Console.WriteLine("Error: Enum values do not match.");
    }
  }

  private static TEnum? GetEnumValue<TEnum>(JsonNode contentfulEntry, string key, TDbEntity dbEntry)
    where TEnum : struct, Enum
  {
    var contentfulValue = contentfulEntry[key]?.GetValue<string>();
    if (!Enum.TryParse(contentfulValue, out TEnum parsedEnum))
    {
      Console.WriteLine($"{contentfulValue} in {dbEntry.Id} not found in enum ${typeof(TEnum)}");
      return null;
    }

    return parsedEnum;
  }

}