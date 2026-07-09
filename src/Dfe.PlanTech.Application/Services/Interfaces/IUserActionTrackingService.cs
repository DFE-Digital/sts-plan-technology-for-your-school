using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IUserActionTrackingService
{
    Task RecordActionAsync();
    Task<SqlUserActionDto?> GetAsync(Guid id);
}
