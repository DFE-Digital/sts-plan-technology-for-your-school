using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class SubmissionInformationModel
{
    public int EstablishmentId { get; set; }
    public string SectionId { get; set; } = null!;
    public int? SubmissionId { get; set; }
    public DateTime? DateCompleted { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateLastUpdated { get; set; }
    public SubmissionStatus Status { get; set; }
}
