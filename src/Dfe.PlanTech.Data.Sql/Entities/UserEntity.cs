using System.ComponentModel.DataAnnotations.Schema;
using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

[Table("user")]
public class UserEntity
{
    public int Id { get; set; }

    public string DfeSignInRef { get; set; } = null!;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    public DateTime? DateLastUpdated { get; set; }

    public List<SignInEntity> SignIns { get; set; } = new();

    public List<ResponseEntity> Responses { get; set; } = new();

    public SqlUserDto AsDto()
    {
        return new SqlUserDto
        {
            Id = Id,
            DfeSignInRef = DfeSignInRef,
            DateCreated = DateCreated,
            DateLastUpdated = DateLastUpdated,
            Responses = Responses.Select(r => r.AsDto()).ToList()
        };
    }
}
