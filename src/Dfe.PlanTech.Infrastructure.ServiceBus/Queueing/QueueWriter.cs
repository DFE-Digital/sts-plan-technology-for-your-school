using Azure.Messaging.ServiceBus;
using Dfe.PlanTech.Infrastructure.ServiceBus.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Infrastructure.ServiceBus.Queueing;

/// <summary>
/// Implementation of <see cref="IQueueWriter"/> for the Azure Service Bus
/// </summary>
/// <param name="serviceBusSenderFactory"></param>
/// <param name="logger"></param>
public class QueueWriter(
    ILogger<QueueWriter> logger,
    IAzureClientFactory<ServiceBusSender> serviceBusSenderFactory
) : IQueueWriter
{
    private readonly ILogger<QueueWriter> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ServiceBusSender _serviceBusSender = serviceBusSenderFactory.CreateClient(
        "contentfulsender"
    );

    public async Task<QueueWriteResult> WriteMessage(string body, string subject)
    {
        try
        {
            var serviceBusMessage = new ServiceBusMessage(body) { Subject = subject };
            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
            return new QueueWriteResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error sending service bus message. Body: \"{Body}\". Subject: \"{Subject}\".",
                body,
                subject
            );
            return new QueueWriteResult(ex.Message);
        }
    }
}
