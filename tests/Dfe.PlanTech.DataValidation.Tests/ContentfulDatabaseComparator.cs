using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.DataValidation.Tests;

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
    Assert.NotEmpty(_dbEntities);
  }

  public void LoadContentfulEntities(ContentfulContent content, string contentType)
  {
    _contentfulEntities = content.GetEntriesForContentType(contentType)
                                              .ToList();
    Assert.NotEmpty(_contentfulEntities);
  }

  public void CompareContent(Action<JsonNode, TDbEntity> contentMatchesPredicate)
  {
    ArgumentNullException.ThrowIfNull(_contentfulEntities);
    ArgumentNullException.ThrowIfNull(_dbEntities);

    foreach (var contentfulEntry in _contentfulEntities!)
    {
      var matchingDbEntry = _dbEntities.FirstOrDefault(dbEntry => dbEntry.Id == contentfulEntry.GetEntryId());

      Assert.NotNull(matchingDbEntry);

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
    // TODO: refactor so that it works with any array and not just sections
    var references = contentfulEntry[arrayKey]?.AsArray();

    if(references == null){
      return;
    }

    foreach (var reference in references)
    {
      var referenceId = reference?["sys"]?["id"]?.GetValue<string>();
      var matchingReference = selectReferences(dbEntry).Find(entry => entry.Id == referenceId);

      Assert.NotNull(matchingReference);
    }
  }

  public void ValidateEnumValue<TEnum>(JsonNode contentfulEntry, string key, TDbEntity dbEntry, TEnum dbValue)
    where TEnum : struct, Enum
  {
    var enumValue = GetEnumValue<TEnum>(contentfulEntry, key, dbEntry);
    Assert.Equal(enumValue, dbValue);
  }

  private TEnum GetEnumValue<TEnum>(JsonNode contentfulEntry, string key, TDbEntity dbEntry)
    where TEnum : struct, Enum
  {
    var contentfulValue = contentfulEntry[key]?.GetValue<string>();
    if (!Enum.TryParse(contentfulValue, out TEnum parsedEnum))
    {
      Assert.Fail($"{contentfulValue} in {dbEntry.Id} not found in enum ${typeof(TEnum)}");
    }

    return parsedEnum;
  }

}