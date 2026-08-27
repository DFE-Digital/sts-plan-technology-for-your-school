using System.ComponentModel.DataAnnotations;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class GiasTypeOfEstablishmentEntity
{
    [Required]
    public int TypeOfEstablishmentCode { get; init; }

    public string TypeOfEstablishmentName { get; init; } = null!;

    public SqlGiasTypeOfEstablishmentDto AsDto()
    {
        return new SqlGiasTypeOfEstablishmentDto
        {
            TypeOfEstablishmentCode = TypeOfEstablishmentCode,
            TypeOfEstablishmentName = TypeOfEstablishmentName,
        };
    }
}
