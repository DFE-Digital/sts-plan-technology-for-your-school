using Dfe.PlanTech.Domain.Models;

namespace Dfe.PlanTech.Domain.DataTransferObjects;

public class RecordUserSignInDto
{
    public required string DfeSignInRef { get; init; }
    public required OrganisationModel Organisation { get; init; }
}
