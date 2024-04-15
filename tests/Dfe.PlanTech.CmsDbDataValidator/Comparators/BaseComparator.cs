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

  protected TDbEntity? FindMatchingDbEntity<TDbEntity>(IEnumerable<TDbEntity> dbEntities, JsonNode contentfulEntity)
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

  protected static string? GetId(JsonNode? entry) => entry == null ? null : entry!["sys"]?["id"]?.GetValue<string>() ?? throw new JsonException($"Couldn't find Id in {entry}");

  private static string LowercaseFirstLetter(string input)
  {
    if (string.IsNullOrEmpty(input))
    {
      return input;
    }
    return char.ToLower(input[0]) + input[1..];
  }
}