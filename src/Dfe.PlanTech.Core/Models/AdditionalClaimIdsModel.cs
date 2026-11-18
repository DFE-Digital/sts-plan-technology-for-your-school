using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public sealed class AdditionalClaimIdsModel
{
    public int? FocusedEstablishmentId { get; init; }
    public int? TrustEstablishmentId { get; init; }
}
