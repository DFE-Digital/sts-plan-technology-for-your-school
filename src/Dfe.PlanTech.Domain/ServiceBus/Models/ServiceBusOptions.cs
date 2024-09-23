namespace Dfe.PlanTech.Domain.ServiceBus.Models;

public record ServiceBusOptions
{
    public int MessagesPerBatch { get; init; } = 10;
    public required string QueueName { get; init; } = "";
}
