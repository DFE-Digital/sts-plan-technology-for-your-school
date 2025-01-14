namespace Dfe.PlanTech.Domain.ServiceBus.Models;

public record ServiceBusOptions
{
    /// <summary>
    /// Enables reading + processing of messages from the Service Bus
    /// </summary>
    public bool EnableQueueReading { get; init; } = true;
}
