using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Application.Users.Interfaces;

public interface IRecordUserSignInCommand
{
    Task<int> RecordSignIn(RecordUserSignInDto recordUserSignInDto);
}
