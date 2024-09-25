namespace Dfe.PlanTech.Domain.ServiceBus.Models;

public record ServiceBusErrorResult(string Reason, string? Description, bool IsRetryable) : IServiceBusResult;
