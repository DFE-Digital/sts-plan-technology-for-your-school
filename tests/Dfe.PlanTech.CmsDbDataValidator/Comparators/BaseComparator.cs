using System.Text.Json;
using System.Text.Json.Nodes;
using Dfe.PlanTech.CmsDbDataValidator.Models;
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

    public readonly EntityTypeValidationErrors EntityTypeValidationErrors = new() { ContentfulContentType = entityType };

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

        EntityTypeValidationErrors.EntityValidationErrors.Capacity = _contentfulEntities.Count;

        return true;
    }

    public async Task<string> ValidateContentAndPrintErrors()
    {
        try
        {
            await ValidateContent();
            string errorMessage = string.Join(Environment.NewLine, [EntityTypeValidationErrors.ToString(), "", "-------------", ""]);

            Console.WriteLine(errorMessage);

            return errorMessage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating {_entityType}: {ex.Message}");
            return ex.Message;
        }
    }

    public abstract Task ValidateContent();

    protected virtual async Task<bool> GetDbEntities()
    {
        try
        {
            _dbEntities = await GetDbEntitiesQuery().ToListAsync();

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

        return CompareStrings(contentfulEntity[contentfulPropertyName]?.GetEntryId(), dbEntityValue);
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

        return CompareStrings(contentfulValue, databaseValue);
    }

    protected DataValidationError? CompareProperty(JsonNode contentfulEntity, ContentComponentDbEntity databaseEntity, string propertyName, string? contentfulPropertyName = null)
    {
        var formattedPropertyName = contentfulPropertyName ?? LowercaseFirstLetter(propertyName);

        var contentfulProperty = contentfulEntity[formattedPropertyName];

        var databaseProperty = databaseEntity.GetType().GetProperty(propertyName);

        if (databaseProperty == null)
        {
            return GenerateDataValidationError(propertyName, "Missing in DB entity");
        }

        var databaseValue = databaseProperty!.GetValue(databaseEntity);

        if (contentfulProperty == null && databaseValue == null)
        {
            return null;
        }
        else if (contentfulProperty == null && databaseValue != null)
        {
            return GenerateDataValidationError(formattedPropertyName, $"{formattedPropertyName} is null in Contentful but is {databaseValue} in the database.");
        }
        else if (contentfulProperty != null && databaseValue == null)
        {
            return GenerateDataValidationError(propertyName, $"{propertyName} is null in DB but is {databaseValue} in the database.");
        }

        var contentfulValue = contentfulProperty!.Deserialize(databaseProperty.PropertyType);

        if (databaseValue?.GetType() == typeof(string) && contentfulValue?.GetType() == typeof(string))
        {
            var stringError = CompareStrings(contentfulValue as string, databaseValue as string);

            if (stringError != null)
            {
                return GenerateDataValidationError(propertyName, stringError);
            }
        }

        var matching = Equals(contentfulValue, databaseValue);

        var errorMessage = GetValidationMessage(contentfulValue, databaseValue, matching);

        if (errorMessage != null)
        {
            return GenerateDataValidationError(propertyName, errorMessage);
        }

        return null;
    }

    protected static string? CompareStrings(string? contentfulValue, string? databaseValue)
    {
        var matches = string.Equals(contentfulValue, databaseValue);
        return GetValidationMessage(contentfulValue, databaseValue, matches);
    }

    private static string? GetValidationMessage<T>(T? contentfulValue, T? databaseValue, bool matches)
      => matches ? null : $"Expected {contentfulValue} but found {databaseValue}";

    protected EntityValidationErrors ValidateProperties(JsonNode contentfulEntity, ContentComponentDbEntity dbEntity, params DataValidationError?[]? extraValidations)
    {
        var validationResults = GetValidationResults(contentfulEntity, dbEntity).Concat(extraValidations ?? [])
                                                                                .Where(validationResult => validationResult != null)!;

        var entityValidationError = GenerateEntityValidationErrors(dbEntity.Id ?? contentfulEntity.GetEntryId()!, validationResults!);

        EntityTypeValidationErrors.AddErrors(entityValidationError);

        return entityValidationError;
    }

    private IEnumerable<DataValidationError?> GetValidationResults(JsonNode contentfulEntity, ContentComponentDbEntity dbEntity)
    => _propertiesToValidate.Select(prop => CompareProperty(contentfulEntity, dbEntity, prop));

    protected void LogValidationMessages(IEnumerable<string?> validationResults, JsonNode contentfulEntity)
    {
        if (validationResults.Any())
        {
            Console.WriteLine($"Validation failures for {_entityType} {contentfulEntity.GetEntryId()}: \n {string.Join("\n", validationResults)}");
        }
    }

    protected TDbEntity? TryRetrieveMatchingDbEntity<TDbEntity>(IEnumerable<TDbEntity> dbEntities, JsonNode contentfulEntity)
      where TDbEntity : ContentComponentDbEntity
    {
        var contentfulEntityId = contentfulEntity.GetEntryId();
        if (string.IsNullOrEmpty(contentfulEntityId))
        {
            Console.WriteLine($"Could not find ID for Contentful Entity {contentfulEntity}");
            return null;
        }

        var databaseEntity = dbEntities.FirstOrDefault(entity => entity.Id == contentfulEntityId);

        if (databaseEntity == null)
        {
            EntityTypeValidationErrors.AddErrors(GenerateEntityValidationErrors(contentfulEntityId, GenerateDataValidationError("ENTITY", "Not found in database")));
            return null;
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
    protected IEnumerable<DataValidationError> ValidateChildren<TDbEntity, TDbEntityReference>(JsonNode contentfulEntity, string arrayKey, TDbEntity dbEntity, Func<TDbEntity, List<TDbEntityReference>> selectReferences)
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
                yield return GenerateDataValidationError(arrayKey, $"No children in Contentful by DB entity has {dbChildren.Count} children");
            }

            yield break;
        }

        var dbEntityType = typeof(TDbEntity).Name.Replace("DbEntity", "");

        var contentfulEntityId = contentfulEntity.GetEntryId();

        var missingDbEntities = contentfulChildrenIds.Where(childId => !dbChildren.Any(dbChild => dbChild.Id == childId))
                                                    .Select(id => GenerateDataValidationError(arrayKey, $"Missing {id} in database"));

        var extraDbEntities = dbChildren.Where(dbChild => !contentfulChildrenIds.Any(childId => dbChild.Id == childId))
                                      .Select(dbChild => GenerateDataValidationError(arrayKey, $"{dbChild.Id} exists in DB but not Contentful"));


        foreach (var missingDbEntity in missingDbEntities)
        {
            yield return missingDbEntity;
        }

        foreach (var extraDbEntity in extraDbEntities)
        {
            yield return extraDbEntity;
        }
    }

    public DataValidationError? ValidateEnumValue<TEnum, TDbEntity>(JsonNode contentfulEntry, string key, TDbEntity? dbEntry, TEnum? dbValue)
      where TEnum : struct, Enum
      where TDbEntity : ContentComponentDbEntity
    {
        var enumValue = GetEnumValue<TEnum>(contentfulEntry, key);
        if (enumValue == null)
        {
            return GenerateDataValidationError(key, $"Enum was not found in the Contentful data");
        }

        if (!enumValue.Equals(dbValue))
        {
            return GenerateDataValidationError(key, $"Expected {enumValue} but DB has {dbValue}");
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

    /// <remarks>
    /// Due to the size of TextBody entities, fetching all in one will cause timeout issue.
    /// Paginate through them instead.
    /// </remarks>
    protected virtual async Task<bool> GetDbEntitiesPaginated(int take = 50)
    {
        try
        {
            var entityCount = await GetDbEntitiesQuery().CountAsync();

            var entities = new List<ContentComponentDbEntity>(entityCount);

            int skip = 0;
            while (true)
            {
                var paginatedEntities = await GetDbEntitiesQuery().Skip(skip).Take(take).ToListAsync();
                entities.AddRange(paginatedEntities);

                skip += take;

                if (skip >= entityCount)
                    break;
            }

            _dbEntities = entities;

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

    private static string LowercaseFirstLetter(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        return char.ToLower(input[0]) + input[1..];
    }

    protected virtual DataValidationError GenerateDataValidationError(string field, string message)
    => new(field, message);

    protected virtual DataValidationError? TryGenerateDataValidationError(string field, string? message)
    => message == null ? null : new(field, message);

    protected virtual EntityValidationErrors GenerateEntityValidationErrors(string entityId, IEnumerable<DataValidationError> dataValidationErrors)
    => new()
    {
        EntityId = entityId,
        Errors = dataValidationErrors.ToList()
    };

    protected virtual EntityValidationErrors GenerateEntityValidationErrors(string entityId, params DataValidationError[] dataValidationErrors)
    => new()
    {
        EntityId = entityId,
        Errors = [.. dataValidationErrors]
    };

}
