namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlGiasEstablishmentDto : ISqlDto
{
    public int Urn { get; init; }
    public Int64? Uprn { get; init; }
    public int? EstablishmentNumber { get; init; }
    public string EstablishmentName { get; init; } = null!;
    public int EstablishmentStatusCode { get; init; }
    public int LocalAuthorityCode { get; init; }
    public int PhaseCode { get; init; }
    public int? TypeOfEstablishmentCode { get; init; }
    public string? Ukprn { get; init; }
    public DateTime SyncedAt { get; init; }
}
