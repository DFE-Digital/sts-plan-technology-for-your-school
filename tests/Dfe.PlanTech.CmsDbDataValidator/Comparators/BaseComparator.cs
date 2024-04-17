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

  protected virtual string? ValidateReferences<TDbEntity>(ContentComponentDbEntity dbEntity, string dbEntityPropertyName, JsonNode contentfulEntity, string contentfulPropertyName)
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

    foreach (var contentfulChildId in contentfulChildrenIds)
    {
      var matchingDbEntity = dbChildren.FirstOrDefault(child => child.Id == contentfulChildId);
      if (matchingDbEntity == null)
      {
        Console.WriteLine($"Could not find matching entity for child ID {contentfulChildId} in DB for {contentfulEntity.GetEntryId()}");
      }
    }

    foreach (var dbChild in dbChildren)
    {
      var matchingContentfulReference = contentfulChildrenIds.FirstOrDefault(id => id == dbChild.Id);
      if (matchingContentfulReference == null)
      {
        Console.WriteLine($"Entity ID {dbChild.Id} is a child for {dbChild.Id} but was not found in the Contentful data as a child");
      }
    }
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