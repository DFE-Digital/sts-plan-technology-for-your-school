using Dfe.PlanTech.Domain.Groups.Models;

namespace Dfe.PlanTech.Domain.Groups.Interfaces;

public interface IGetGroupSelectionQuery
{
    Task<GroupReadActivityDto?> GetLatestSelectedGroupSchool(int userId, int userEstablishmentId, CancellationToken cancellationToken = default);
}

