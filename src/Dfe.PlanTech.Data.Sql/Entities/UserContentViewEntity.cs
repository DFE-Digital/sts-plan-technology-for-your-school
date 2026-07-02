using Dfe.PlanTech.Data.Sql.Interfaces;

namespace Dfe.PlanTech.Data.Sql.Entities;

public class UserContentViewEntity : IUserActionEntity
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public Guid? UserActionId { get; set; }
    public required string ContentfulRef { get; set; }
}
