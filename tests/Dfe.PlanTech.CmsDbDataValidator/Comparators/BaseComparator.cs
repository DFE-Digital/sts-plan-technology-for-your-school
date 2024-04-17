using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public abstract class BaseComparator(CmsDbContext db, ContentfulContent contentfulContent, string[] propertiesToValidate, string entityType)
{
  protected readonly string _entityType = entityType;
  protected readonly string[] _propertiesToValidate = propertiesToValidate;
  protected readonly CmsDbContext _db = db;
  protected readonly ContentfulContent _contentfulContent = contentfulContent;

  protected List<JsonNode> _contentfulEntities = [];
  protected List<ContentComponentDbEntity> _dbEntities = [];

  public virtual async Task<bool> InitialiseContent()
  {
    if (!GetContentfulEntities())
    {
      return false;
    }

    if (!await GetDbEntities())
    {
      return false;
    }

    return true;
  }

  public abstract Task ValidateContent();

  protected virtual async Task<bool> GetDbEntities()
  {
    _dbEntities = await GetDbEntitiesQuery().ToListAsync();

    if (_dbEntities == null || _dbEntities.Count == 0)
    {
      Console.WriteLine($"{_entityType}s not found in database");
      return false;
    }

    return true;
  }

  protected abstract IQueryable<ContentComponentDbEntity> GetDbEntitiesQuery();

  protected virtual string? ValidateChild<TDbEntity>(ContentComponentDbEntity dbEntity, string dbEntityPropertyName, JsonNode contentfulEntity, string contentfulPropertyName)
    where TDbEntity : ContentComponentDbEntity
  {
    var databaseProperty = typeof(TDbEntity).GetProperties().FirstOrDefault(prop => prop.Name == dbEntityPropertyName);

    if (databaseProperty == null)
    {
      Console.WriteLine($"{typeof(TDbEntity)} does not have property {dbEntityPropertyName}");
      return null;
    }

    var dbEntityValue = databaseProperty.GetValue(dbEntity)?.ToString();

    return CompareStrings(dbEntityPropertyName, contentfulEntity[contentfulPropertyName]?.GetEntryId(), dbEntityValue);
  }

  protected virtual bool GetContentfulEntities()
  {
    _contentfulEntities = _contentfulContent.GetEntriesForContentType(LowercaseFirstLetter(_entityType)).ToList();

    if (_contentfulEntities == null || _contentfulEntities.Count == 0)
    {
      Console.WriteLine($"{_entityType}s not found in Contentful export");
      return false;
    }

    return true;
  }

  protected static string? CompareStringProperty(JsonNode contentfulEntity, ContentComponentDbEntity databaseEntity, string propertyName, string? contentfulPropertyName = null)
  {
    var formattedPropertyName = contentfulPropertyName ?? LowercaseFirstLetter(propertyName);
    var contentfulValue = contentfulEntity[formattedPropertyName]?.GetValue<string>();
    var databaseValue = databaseEntity.GetType().GetProperty(propertyName)?.GetValue(databaseEntity)?.ToString();

    return CompareStrings(propertyName, contentfulValue, databaseValue);
  }

  protected static string? CompareProperty(JsonNode contentfulEntity, ContentComponentDbEntity databaseEntity, string propertyName, string? contentfulPropertyName = null)
  {
    var formattedPropertyName = contentfulPropertyName ?? LowercaseFirstLetter(propertyName);

    var contentfulProperty = contentfulEntity[formattedPropertyName];

    var databaseProperty = databaseEntity.GetType().GetProperty(propertyName);

    if (databaseProperty == null)
    {
      return $"Could not find property {propertyName} in {databaseEntity.GetType()}";
    }

    var databaseValue = databaseProperty!.GetValue(databaseEntity);

    if (contentfulProperty == null && databaseValue == null)
    {
      return null;
    }
    else if (contentfulProperty == null && databaseValue != null)
    {
      return $"{formattedPropertyName} is null in Contentful but is {databaseValue} in the database.";
    }
    else if (contentfulProperty != null && databaseValue == null)
    {
      return $"{propertyName} is null in DB but is {databaseValue} in the database.";
    }

    var contentfulValue = contentfulProperty!.Deserialize(databaseProperty.PropertyType);

    if (databaseValue?.GetType() == typeof(string) && contentfulValue?.GetType() == typeof(string))
    {
      return CompareStrings(propertyName, contentfulValue as string, databaseValue as string);
    }

    var matching = Equals(contentfulValue, databaseValue);

    return GetValidationMessage(propertyName, contentfulValue, databaseValue, matching);
  }

  protected static string? CompareStrings(string propertyName, string? contentfulValue, string? databaseValue)
  {
    var matches = string.Equals(contentfulValue, databaseValue);
    return GetValidationMessage(propertyName, contentfulValue, databaseValue, matches);
  }

  private static string? GetValidationMessage<T>(string propertyName, T? contentfulValue, T? databaseValue, bool matches)
    => matches ? null : $"{propertyName} doesn't match. Expected {contentfulValue} but found {databaseValue}";

  protected void ValidateProperties(JsonNode contentfulEntity, ContentComponentDbEntity dbEntity, params string?[]? extraValidations)
  {
    var validationResults = GetValidationResults(contentfulEntity, dbEntity).Concat(extraValidations ?? [])
                                                                            .Where(validationResult => validationResult != null)!;

    LogValidationMessages(validationResults, contentfulEntity);
  }

  private IEnumerable<string?> GetValidationResults(JsonNode contentfulEntity, ContentComponentDbEntity dbEntity)
  => _propertiesToValidate.Select(prop => CompareProperty(contentfulEntity, dbEntity, prop));

  protected void LogValidationMessages(IEnumerable<string?> validationResults, JsonNode contentfulEntity)
  {
    if (validationResults.Any())
    {
      Console.WriteLine($"Validation failures for {_entityType} {contentfulEntity.GetEntryId()}: \n {string.Join("\n", validationResults)}");
    }
  }

  protected TDbEntity? ValidateChildEntityExistsInDb<TDbEntity>(IEnumerable<TDbEntity> dbEntities, JsonNode contentfulEntity)
    where TDbEntity : ContentComponentDbEntity
  {
    var contentfulEntityId = contentfulEntity.GetEntryId();
    var databaseEntity = dbEntities.FirstOrDefault(entity => entity.Id == contentfulEntityId);

    if (databaseEntity == null)
    {
      Console.WriteLine($"Could not find matching {_entityType} in DB for {contentfulEntityId}");
    }

    return databaseEntity;
  }

  /// <summary>
  /// Validates array references for a given contentful entry and db entry.
  /// </summary>
  /// <typeparam name="TDbEntityReferences">The type of the db entity references.</typeparam>
  /// <param name="contentfulEntity">The contentful entry to validate.</param>
  /// <param name="arrayKey">The key of the array to validate.</param>
  /// <param name="dbEntity">The db entry to use for validation.</param>
  /// <param name="selectReferences">A function to select references from the db entry.</param>
  protected void ValidateChildren<TDbEntity, TDbEntityReference>(JsonNode contentfulEntity, string arrayKey, TDbEntity dbEntity, Func<TDbEntity, List<TDbEntityReference>> selectReferences)
      where TDbEntity : ContentComponentDbEntity
      where TDbEntityReference : ContentComponentDbEntity
  {
    var dbChildren = selectReferences(dbEntity);

    // TODO: refactor so that it works with any array and not just sections
    var contentfulChildrenIds = contentfulEntity[arrayKey]?.AsArray()
                                                        .Where(child => child != null)
                                                        .Select(child => child?.GetEntryId())
                                                        .ToArray();

    if (contentfulChildrenIds == null || contentfulChildrenIds.Length == 0)
    {
      //If we have no Contentful children, but we have children in DB, then something's a bit screwy somewhere
      if (dbChildren != null && dbChildren.Count > 0)
      {
        Console.WriteLine($"Contentful entity {contentfulEntity.GetEntryId()} has no children but DB entity has {dbChildren.Count} children");
      }

      return;
    }

    var dbEntityType = typeof(TDbEntity).Name.Replace("DbEntity", "");

    var contentfulEntityId = contentfulEntity.GetEntryId();

    var missingDbEntities = contentfulChildrenIds.Where(childId => !dbChildren.Any(dbChild => dbChild.Id == childId)).ToArray();

    var extraDbEntities = dbChildren.Where(dbChild => !contentfulChildrenIds.Any(childId => dbChild.Id == childId))
                                    .Select(dbChild => dbChild.Id)
                                    .ToArray();


    var missingDbEntitiesErrorMessage = missingDbEntities.Length != 0 ? $"  IDs missing in DB but exist in Contentful: \n     {string.Join("\n    ", missingDbEntities)}" : null;
    var extraDbEntitiesErrorMessage = extraDbEntities.Length != 0 ? $"  IDs in DB but not in Contentful: \n      {string.Join("\n    ", extraDbEntities)}" : null;

    IEnumerable<string?> nonNullErrors = [extraDbEntitiesErrorMessage, extraDbEntitiesErrorMessage];

    var childErrors = string.Join("\n", nonNullErrors.Where(error => error != null));

    if (!string.IsNullOrEmpty(childErrors))
    {
      Console.WriteLine($"Child reference errors in {dbEntityType} {contentfulEntityId} for property {arrayKey}: \n{childErrors}");
    }
  }

  public string? ValidateEnumValue<TEnum, TDbEntity>(JsonNode contentfulEntry, string key, TDbEntity dbEntry, TEnum dbValue)
    where TEnum : struct, Enum
    where TDbEntity : ContentComponentDbEntity
  {
    var enumValue = GetEnumValue<TEnum>(contentfulEntry, key);
    if (enumValue == null)
    {
      return $"Enum {key} in {contentfulEntry.GetEntryId()} was not found";
    }

    if (!enumValue.Equals(dbValue))
    {
      return $"Error: Enum values do not match. Expected {enumValue} but DB has {dbValue}";
    }

    return null;
  }

  private static TEnum? GetEnumValue<TEnum>(JsonNode contentfulEntry, string key)
    where TEnum : struct, Enum
  {
    var contentfulValue = contentfulEntry[key]?.GetValue<string>();
    if (!Enum.TryParse(contentfulValue, out TEnum parsedEnum))
    {
      return null;
    }

    return parsedEnum;
  }


  private static string LowercaseFirstLetter(string input)
  {
    if (string.IsNullOrEmpty(input))
    {
      return input;
    }
    return char.ToLower(input[0]) + input[1..];
  }
}