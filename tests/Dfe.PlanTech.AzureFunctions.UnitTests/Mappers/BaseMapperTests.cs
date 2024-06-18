using Dfe.PlanTech.Domain.Caching.Models;
using Dfe.PlanTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Dfe.PlanTech.AzureFunctions;

public abstract class BaseMapperTests
{
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

    protected static void MockEntityEntry(CmsDbContext db, params Type[] types)
    {
        var model = Substitute.For<IModel>();

        List<IEntityType> mocks = types.Select(type =>
        {
            var entityTypeMock = Substitute.For<IEntityType>();
            entityTypeMock.ClrType.Returns(type);

            return entityTypeMock;
        }).ToList();

        model.FindEntityType(Arg.Any<Type>()).Returns((callinfo) =>
        {
            var type = callinfo.ArgAt<Type>(0);

            var matching = mocks.FirstOrDefault(mock => mock.ClrType == type);

            if (matching != null)
            {
                return matching;
            }

            throw new NotImplementedException($"Not expecting type {type.Name}");
        });

        db.Model.Returns(model);
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
}