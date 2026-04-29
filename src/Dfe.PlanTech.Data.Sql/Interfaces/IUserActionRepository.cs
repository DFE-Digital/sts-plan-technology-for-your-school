using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces;

public interface IUserActionRepository
{
    Task CreateAsync(UserActionEntity userAction);
}
