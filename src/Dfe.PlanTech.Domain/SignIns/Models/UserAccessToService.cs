namespace Dfe.PlanTech.Domain.SignIns.Models;

public sealed class UserAccessToService
{
    public Guid UserId { get; set; }

    public Guid ServiceId { get; set; }

    public Guid OrganisationId { get; set; }

    public IList<Role> Roles { get; set; } = null!;

    public IList<Identifier> Identifiers { get; set; } = null!;

    public IList<Status> Status { get; set; } = null!;
}