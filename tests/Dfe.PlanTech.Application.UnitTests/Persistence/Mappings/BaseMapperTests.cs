using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Mappings;
using Dfe.PlanTech.Application.UnitTests.TestHelpers;
using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Mappings;

public abstract class BaseMapperTests<TEntity, TMapper> : BaseMapperTests
    where TEntity : ContentComponentDbEntity, new()
    where TMapper : BaseJsonToDbMapper<TEntity>
{
    protected readonly ILogger<TMapper> Logger = Substitute.For<ILogger<TMapper>>();

    protected BaseMapperTests()
    {
    }
}


public abstract class BaseMapperTests
{
    protected readonly IDatabaseHelper<ICmsDbContext> DatabaseHelper = Substitute.For<IDatabaseHelper<ICmsDbContext>>();
    protected readonly ICmsDbContext Db = Substitute.For<ICmsDbContext>();
    protected readonly ILogger<EntityUpdater> EntityUpdaterLogger = Substitute.For<ILogger<EntityUpdater>>();
    protected readonly EntityUpdater EntityUpdater;

    protected BaseMapperTests()
    {
        EntityUpdater = new EntityUpdater(EntityUpdaterLogger, DatabaseHelper);

        DatabaseHelper.Database.Returns(Db);
    }

    /// <summary>
    /// Validates that all reference IDs exist in the references, and validates order matches if required.
    /// </summary>
    /// <typeparam name="TReferencedContentComponent"></typeparam>
    /// <param name="referenceIds"></param>
    /// <param name="references"></param>
    /// <param name="validateOrder"></param>
    /// <param name="expectedOrder">
    /// By default, the order of the referenced content is expected to match the order of the reference IDs.
    /// If this field is not null order will be validated against this instead
    /// </param>
    protected static void ValidateReferencedContent<TReferencedContentComponent>(string[] referenceIds, List<TReferencedContentComponent>? references, bool validateOrder, int? expectedOrder = null)
    where TReferencedContentComponent : ContentComponentDbEntity
    {
        Assert.NotNull(references);
        Assert.Equal(referenceIds.Length, references.Count);

        for (int x = 0; x < referenceIds.Length; x++)
        {
            var referenceId = referenceIds[x];
            var matching = references.FirstOrDefault(answer => answer.Id == referenceId);
            Assert.NotNull(matching);

            if (validateOrder)
            {
                Assert.Equal(expectedOrder ?? x, matching.Order);
            }
        }
    }

    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static Dictionary<string, object?> WrapWithLocalisation(object? toWrap, string localisation = "en-US")
    => new()
    {
        [localisation] = toWrap
    };

    protected CmsWebHookPayload CreatePayload(Dictionary<string, object?> fields, string entityId)
    {
        var asJson = JsonSerializer.Serialize(fields, JsonOptions);
        var asJsonNode = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(asJson, JsonOptions);

        var payload = new CmsWebHookPayload()
        {
            Sys = new CmsWebHookSystemDetails()
            {
                Id = entityId,
                Type = "Entry"
            },
            Fields = asJsonNode!
        };
        return payload;
    }

    protected static void FindLogMessagesContainingStrings(ILogger logger, params string[] substrings)
    {
        var receivedLogCalls = logger.ReceivedCalls();

        var loggedErrorMessages = receivedLogCalls.Select(logCall =>
        {
            var arguments = logCall.GetArguments();
            Assert.NotNull(arguments);
            var errorMessage = arguments[2]!.ToString();
            Assert.NotNull(errorMessage);
            Assert.NotEmpty(errorMessage);
            return errorMessage;
        }).ToArray();

        foreach (var substring in substrings)
        {
            var matchingLoggedErrorMessage = loggedErrorMessages.FirstOrDefault(message => message.Contains(substring));

            Assert.NotNull(matchingLoggedErrorMessage);
        }
    }

    protected static CmsWebHookSystemDetailsInnerContainer CreateReferenceInnerForId(string id)
    => new()
    {
        Sys = new CmsWebHookSystemDetailsInner()
        {
            Id = id
        }
    };

    protected void MockDatabaseCollection<T>(List<T> sourceEntities) where T : class
        => DatabaseHelper.MockDatabaseCollection(sourceEntities);
}
