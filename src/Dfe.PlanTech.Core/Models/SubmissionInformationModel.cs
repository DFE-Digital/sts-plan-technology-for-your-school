using Dfe.PlanTech.Core.Contentful.Models;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;
using Dfe.PlanTech.Core.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.PlanTech.Core.Models;

[ExcludeFromCodeCoverage]
public class SubmissionInformationModel
{
    public int EstablishmentId { get; set; }
    public string EstablishmentName { get; set; } = null!;
    public string EstablishmentRef { get; set; } = null!;
    public string SectionId { get; set; } = null!;
    public int? SubmissionId { get; set; }
    public string? DateCompleted { get; set; }
    public string? DateCreated { get; set; }
    public string? DateLastUpdated { get; set; }
    public SubmissionStatus Status { get; set; }
}
