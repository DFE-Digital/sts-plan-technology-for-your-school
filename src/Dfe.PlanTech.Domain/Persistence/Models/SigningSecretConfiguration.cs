namespace Dfe.PlanTech.Domain.Persistence.Models;

public record SigningSecretConfiguration
{
    public string SigningSecret { get; init; } = null!;
}
