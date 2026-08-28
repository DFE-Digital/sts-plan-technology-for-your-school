namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlGiasTypeOfEstablishmentDto : ISqlDto
{
    public int TypeOfEstablishmentCode { get; init; }

    public string TypeOfEstablishmentName { get; init; } = null!;
}
