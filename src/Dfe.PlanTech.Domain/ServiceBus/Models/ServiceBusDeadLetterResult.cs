namespace Dfe.PlanTech.Domain.ServiceBus.Models;

public record ServiceBusDeadLetterResult(string Reason, string? Description, bool IsRetryable) : IServiceBusResult;