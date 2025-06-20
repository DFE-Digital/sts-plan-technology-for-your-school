using Dfe.PlanTech.Domain.DataTransferObjects;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class GroupReadActivityEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int UserEstablishmentId { get; set; }

    public int SelectedEstablishmentId { get; set; }

    public string SelectedEstablishmentName { get; set; } = string.Empty;

    public DateTime DateSelected { get; set; }

    public GroupReadActivityDto ToDto()
    {
        return new GroupReadActivityDto
        {
            Id = Id,
            UserId = UserId,
            UserEstablishmentId = UserEstablishmentId,
            SelectedEstablishmentId = SelectedEstablishmentId,
            SelectedEstablishmentName = SelectedEstablishmentName,
            DateSelected = DateSelected
        };
    }
}
