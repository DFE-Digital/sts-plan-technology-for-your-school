using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class UserGroupSelectionModel
{
    public int UserEstablishmentId { get; set; }
    public int SelectedEstablishmentId { get; set; }
    public string? SelectedEstablishmentName { get; set; }
    public int UserId { get; set; }
}
