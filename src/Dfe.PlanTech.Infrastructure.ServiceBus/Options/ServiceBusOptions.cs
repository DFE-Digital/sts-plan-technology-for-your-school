namespace Dfe.PlanTech.Infrastructure.ServiceBus.Options;

public record ServiceBusOptions
{
    /// <summary>
    /// Enables reading + processing of messages from the Service Bus
    /// </summary>
    public bool EnableQueueReading { get; init; } = true;
}
