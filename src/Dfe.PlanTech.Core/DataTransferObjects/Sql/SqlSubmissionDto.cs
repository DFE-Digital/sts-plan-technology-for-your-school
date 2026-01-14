using Dfe.PlanTech.Core.Enums;

namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlSubmissionDto : ISqlDto
{
    public int Id { get; set; }
    public int EstablishmentId { get; set; }
    public SqlEstablishmentDto Establishment { get; set; } = null!;
    public string SectionId { get; set; } = null!;
    public string SectionName { get; set; } = null!;
    public string? Maturity { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateLastUpdated { get; set; }
    public DateTime? DateCompleted { get; set; }
    public IEnumerable<SqlResponseDto> Responses { get; set; } = [];
    public bool Deleted { get; set; }
    public bool Viewed { get; set; }
    public SubmissionStatus Status { get; set; }
}
