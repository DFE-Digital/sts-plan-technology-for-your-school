using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Application.Configuration;

[ExcludeFromCodeCoverage]
public record SigningSecretConfiguration
{
    public string SigningSecret { get; init; } = null!;
}
