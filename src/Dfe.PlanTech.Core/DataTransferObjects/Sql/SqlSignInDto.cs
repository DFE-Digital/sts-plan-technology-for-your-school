namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlSignInDto : SqlDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? EstablishmentId { get; set; }
    public DateTime SignInDateTime { get; set; }
    public SqlUserDto User { get; set; } = default!;
}
