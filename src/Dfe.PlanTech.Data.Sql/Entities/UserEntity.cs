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

    public ICollection<SignInEntity> SignIns { get; set; } = [];

    public ICollection<ResponseEntity>? Responses { get; set; } = [];

    public SqlUserDto AsDto()
    {
        return new SqlUserDto
        {
            Id = Id,
            DfeSignInRef = DfeSignInRef,
            DateCreated = DateCreated,
            DateLastUpdated = DateLastUpdated,
            Responses = Responses?.Select(r => r.AsDto()).ToList()
        };
    }
}
