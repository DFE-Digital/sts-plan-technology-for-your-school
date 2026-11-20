using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IGroupReadActivityRepository
{
    Task<int> RecordGroupSelectionAsync(UserGroupSelectionModel userGroupSelectionModel);
}
