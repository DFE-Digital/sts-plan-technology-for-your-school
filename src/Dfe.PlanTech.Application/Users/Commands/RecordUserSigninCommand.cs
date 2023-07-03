using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Application.Users.Commands;

public class RecordUserSignInCommand : IRecordUserSignInCommand
{
    private readonly IUsersDbContext _db;

    public RecordUserSignInCommand(IUsersDbContext db)
    {
        _db = db;
    }

    public async Task<int> RecordSignIn(RecordUserSignInDto recordUserSignInDto)
    {
        //Check user exists already
        var getUserIdQuery = new GetUserIdQuery(_db);

        var existingUserId = await getUserIdQuery.GetUserId(recordUserSignInDto.DfeSignInRef);

        if (existingUserId == null)
        {
            var CreateUserCommand = new CreateUserCommand(_db);
            await CreateUserCommand.CreateUser(recordUserSignInDto);
            existingUserId = await getUserIdQuery.GetUserId(recordUserSignInDto.DfeSignInRef);
        }

        var signInId = await AddSignInDetails(MapToSignIn(existingUserId));
        return signInId;
    }

    private static Domain.SignIn.Models.SignIn MapToSignIn(int? userId)
    {
        return new Domain.SignIn.Models.SignIn
        {
            UserId = Convert.ToUInt16(userId),
            EstablishmentId = 1, //Replace with value
            SignInDateTime = DateTime.UtcNow
        };
    }

    private async Task<int> AddSignInDetails(Domain.SignIn.Models.SignIn signIn) {
        _db.AddSignIn(signIn);
        var signInId = await _db.SaveChangesAsync();

        return signInId;
    }
}