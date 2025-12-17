namespace Dfe.PlanTech.Core.Models;

public class UserGroupSelectionModel
{
    public int UserEstablishmentId { get; set; }
    public int SelectedEstablishmentId { get; set; }
    public string? SelectedEstablishmentName { get; set; }
    public int UserId { get; set; }
}
