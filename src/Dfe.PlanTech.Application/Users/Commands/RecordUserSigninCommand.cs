using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Application.Users.Commands;

public class RecordUserSignInCommand : IRecordUserSignInCommand
{
    private readonly IPlanTechDbContext _db;
    private readonly ICreateUserCommand _createUserCommand;

    public RecordUserSignInCommand(IPlanTechDbContext db, ICreateUserCommand createUserCommand)
    {
        _db = db;
        _createUserCommand = createUserCommand;
    }

    public async Task<int> RecordSignIn(RecordUserSignInDto recordUserSignInDto)
    {
        //Check user exists already
        var getUserIdQuery = new GetUserIdQuery(_db);
        var existingUserId = await getUserIdQuery.GetUserId(recordUserSignInDto.DfeSignInRef);

        if (existingUserId == null)
        {
            existingUserId = await _createUserCommand.CreateUser(recordUserSignInDto);
        }

        var signInId = await AddSignInDetails(MapToSignIn(existingUserId));
        return signInId;
    }

    private static Domain.SignIn.Models.SignIn MapToSignIn(int? userId, int establishmentId = 1)
    {
        if (userId is null || userId == 0)
            throw new ArgumentNullException(nameof(userId), "User id cannot be null");

        return new Domain.SignIn.Models.SignIn
        {
            UserId = Convert.ToUInt16(userId),
            EstablishmentId = establishmentId,
            SignInDateTime = DateTime.UtcNow
        };
    }

    private async Task<int> AddSignInDetails(Domain.SignIn.Models.SignIn signIn)
    {
        _db.AddSignIn(signIn);
        var signInId = await _db.SaveChangesAsync();

        return signInId;
    }
}