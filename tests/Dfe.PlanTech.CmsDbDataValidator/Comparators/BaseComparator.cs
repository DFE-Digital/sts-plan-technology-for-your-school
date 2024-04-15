using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Infrastructure.Data;

namespace Dfe.PlanTech.CmsDbDataValidator.Tests;

public abstract class BaseComparator(CmsDbContext db, ContentfulContent contentfulContent, string[] propertiesToValidate)
{
  protected readonly string[] _propertiesToValidate = propertiesToValidate;
  protected readonly CmsDbContext _db = db;
  protected readonly ContentfulContent _contentfulContent = contentfulContent;

  public abstract Task ValidateContent();

  protected IEnumerable<string> ValidateProperties(JsonNode contentfulEntity, ContentComponentDbEntity dbEntity)
    => _propertiesToValidate.Select(prop => CompareProperty(contentfulEntity, dbEntity, prop))
                            .Where(prop => prop != null)!;

  protected static string? CompareProperty(JsonNode contentfulEntity, ContentComponentDbEntity databaseEntity, string propertyName, string? contentfulPropertyName = null)
  {
    var formattedPropertyName = contentfulPropertyName ?? LowercaseFirstLetter(propertyName);
    var contentfulValue = contentfulEntity[formattedPropertyName]?.GetValue<string>();
    var databaseValue = databaseEntity.GetType().GetProperty(propertyName)?.GetValue(databaseEntity)?.ToString();

    return CompareStrings(propertyName, contentfulValue, databaseValue);
  }

  protected static string? CompareStrings(string propertyName, string? contentfulValue, string? databaseValue)
  {
    var matches = string.Equals(contentfulValue, databaseValue);

    return matches ? null : $"{propertyName} doesn't match. Expected {contentfulValue} but found {databaseValue}";
  }

  protected static void LogValidationMessages(string entityType, string?[] validationResults, JsonNode contentfulEntity)
  {
    if (validationResults.Any(result => result != null))
    {
      Console.WriteLine($"Validation failures for {entityType} {contentfulEntity.GetEntryId()}: \n {string.Join("\n", validationResults)}");
    }
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