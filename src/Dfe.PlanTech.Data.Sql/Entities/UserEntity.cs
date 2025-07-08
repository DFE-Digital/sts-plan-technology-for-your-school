using Dfe.PlanTech.Core.DataTransferObjects.Sql;

namespace Dfe.PlanTech.Data.Sql.Entities;

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
            SignIns = SignIns.Select(si => si.AsDto()).ToList(),
            Responses = Responses.Select(r => r.AsDto()).ToList()
        };
    }
}
