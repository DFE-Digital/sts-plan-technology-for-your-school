using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Groups.Interfaces;
using Dfe.PlanTech.Domain.Groups.Models;

namespace Dfe.PlanTech.Application.Groups.Interfaces;

public class GetGroupSelectionQuery(IPlanTechDbContext db) : IGetGroupSelectionQuery
{
    public async Task<GroupReadActivityDto?> GetLatestSelectedGroupSchool(int userId, int userEstablishmentId, CancellationToken cancellationToken)
    {
        var query = db.GetGroupReadActivities
            .Where(x => x.UserId == userId && x.UserEstablishmentId == userEstablishmentId)
            .OrderByDescending(x => x.DateSelected)
            .Select(x => new GroupReadActivityDto
            {
                SelectedEstablishmentId = x.SelectedEstablishmentId,
                SelectedEstablishmentName = x.SelectedEstablishmentName
            });

        return await db.FirstOrDefaultAsync(query, cancellationToken);
    }
}
