namespace Dfe.PlanTech.Domain.Establishments.Models;

public class EstablishmentDto
{
    public string EstablishmentRef { get; set; } = null!;
        
    public string EstablishmentType { get; set; } = null!;
        
    public string OrgName { get; set; } = null!;
    
    public DateTime? DateLastUpdated { get; set; }
}