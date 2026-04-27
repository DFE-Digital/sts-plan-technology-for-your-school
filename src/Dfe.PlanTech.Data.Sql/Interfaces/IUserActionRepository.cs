using Dfe.PlanTech.Data.Sql.Entities;

public interface IUserActionRepository
{
    Task CreateAsync(UserActionEntity userAction);
}
