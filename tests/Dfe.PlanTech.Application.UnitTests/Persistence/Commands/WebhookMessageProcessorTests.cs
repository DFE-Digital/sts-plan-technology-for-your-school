using System.Text.Json;
using System.Text.Json.Serialization;
using Dfe.PlanTech.Application.Caching.Interfaces;
using Dfe.PlanTech.Application.Persistence.Commands;
using Dfe.PlanTech.Domain.ServiceBus.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.Application.UnitTests.Persistence.Commands;

public class WebhookMessageProcessorTests
{
    private const string QuestionJsonBody =
        "{\"metadata\":{\"tags\":[]},\"fields\":{\"internalName\":{\"en-US\":\"TestingQuestion\"},\"text\":{\"en-US\":\"TestingQuestion\"},\"helpText\":{\"en-US\":\"HelpText\"},\"answers\":{\"en-US\":[{\"sys\":{\"type\":\"Link\",\"linkType\":\"Entry\",\"id\":\"4QscetbCYG4MUsGdoDU0C3\"}}]},\"slug\":{\"en-US\":\"testing-slug\"}},\"sys\":{\"type\":\"Entry\",\"id\":\"2VSR0emw0SPy8dlR9XlgfF\",\"space\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"Space\",\"id\":\"py5afvqdlxgo\"}},\"environment\":{\"sys\":{\"id\":\"dev\",\"type\":\"Link\",\"linkType\":\"Environment\"}},\"contentType\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"ContentType\",\"id\":\"question\"}},\"createdBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"5yhMQOCN9P2vGpfjyZKiey\"}},\"updatedBy\":{\"sys\":{\"type\":\"Link\",\"linkType\":\"User\",\"id\":\"4hiJvkyVWdhTt6c4ZoDkMf\"}},\"revision\":13,\"createdAt\":\"2023-12-04T14:36:46.614Z\",\"updatedAt\":\"2023-12-15T16:16:45.034Z\"}}";

    private const string QuestionId = "2VSR0emw0SPy8dlR9XlgfF";

    private readonly ILogger<WebhookMessageProcessor> _logger = Substitute.For<ILogger<WebhookMessageProcessor>>();
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ICmsCache _cache = Substitute.For<ICmsCache>();

    private readonly WebhookMessageProcessor _webhookMessageProcessor;

    public WebhookMessageProcessorTests()
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };

        _webhookMessageProcessor = CreateWebhookToDbCommand(true);
    }

    private WebhookMessageProcessor CreateWebhookToDbCommand(bool usePreview) => new(_cache, _jsonSerializerOptions, _logger);

    [Fact]
    public async Task ProcessMessage_Should_Execute_Successfully()
    {
        var subject = "ContentManagement.Entry.save";

        var result = await _webhookMessageProcessor.ProcessMessage(subject, QuestionJsonBody, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusSuccessResult>(result);
        await _cache.Received(1).InvalidateCacheAsync(QuestionId, "Question");
    }

    [Fact]
    public async Task ProcessMessage_Should_DeadLetter_Failed_Operation()
    {
        string nonMappableJson = "\"INVALID\":\"CONTENT\"";

        var subject = "ContentManagement.Entry.save";

        var result = await _webhookMessageProcessor.ProcessMessage(subject, nonMappableJson, "message id", CancellationToken.None);

        Assert.IsType<ServiceBusErrorResult>(result);

        var errorResult = (ServiceBusErrorResult)result;
        Assert.False(errorResult.IsRetryable);
    }
}
