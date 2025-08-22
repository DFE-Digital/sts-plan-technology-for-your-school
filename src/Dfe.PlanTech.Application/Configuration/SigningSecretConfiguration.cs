namespace Dfe.PlanTech.Application.Configuration;

public record SigningSecretConfiguration
{
    public string SigningSecret { get; init; } = null!;
}
