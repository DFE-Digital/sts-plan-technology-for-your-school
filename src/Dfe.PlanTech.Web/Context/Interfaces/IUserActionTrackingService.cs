using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Web.Context.Interfaces;

public interface IUserActionTrackingService
{
    Task RecordAsync();
    Task<SqlUserActionDto?> GetAsync(Guid id);

}
