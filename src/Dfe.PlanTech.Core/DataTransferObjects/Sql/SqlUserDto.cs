namespace Dfe.PlanTech.Core.DataTransferObjects.Sql;

public class SqlUserDto : ISqlDto
{
    public int Id { get; set; }
    public string DfeSignInRef { get; set; } = null!;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? DateLastUpdated { get; set; }
    public List<SqlResponseDto>? Responses { get; set; }
}
