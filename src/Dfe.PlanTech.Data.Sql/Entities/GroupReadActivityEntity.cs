using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("groupReadActivity")]
public class GroupReadActivityEntity : IUserActionEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int UserEstablishmentId { get; set; }

    public int SelectedEstablishmentId { get; set; }

    public string SelectedEstablishmentName { get; set; } = string.Empty;

    public DateTime DateSelected { get; set; }

    public Guid? UserActionId { get; set; }

    public SqlGroupReadActivityDto AsDto()
    {
        return new SqlGroupReadActivityDto
        {
            Id = Id,
            UserId = UserId,
            UserEstablishmentId = UserEstablishmentId,
            SelectedEstablishmentId = SelectedEstablishmentId,
            SelectedEstablishmentName = SelectedEstablishmentName,
            DateSelected = DateSelected,
            UserActionId = UserActionId
        };
    }
}
