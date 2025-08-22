using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Data.Sql.Interfaces
{
    public interface ISignInRepository
    {
        Task<SignInEntity> CreateSignInAsync(int userId, int? establishmentId = null);
        Task<SignInEntity> RecordSignInWithoutEstablishmentIdAsync(string dfeSignInRef);
    }
}
