using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Infrastructure.Data.Sql.Entities;

public class GroupReadActivityEntity : SqlEntity<SqlGroupReadActivityDto>
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int UserEstablishmentId { get; set; }

    public int SelectedEstablishmentId { get; set; }

    public string SelectedEstablishmentName { get; set; } = string.Empty;

    public DateTime DateSelected { get; set; }

    protected override SqlGroupReadActivityDto CreateDto()
    {
        return new SqlGroupReadActivityDto
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
