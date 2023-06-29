using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Application.Users.Interfaces;

public interface IRecordUserSignInCommand
{
    Task RecordSignIn(RecordUserSignInDto recordUserSignInDto);
}
