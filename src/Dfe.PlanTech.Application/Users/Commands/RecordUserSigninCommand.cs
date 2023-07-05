using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Users.Interfaces;
using Dfe.PlanTech.Application.Users.Queries;
using Dfe.PlanTech.Domain.SignIn.Models;
using Dfe.PlanTech.Domain.Users.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Users.Commands;

public class RecordUserSignInCommand : IRecordUserSignInCommand
{
    private readonly IUsersDbContext _db;
    private readonly ICreateUserCommand _createUserCommand;
    private readonly ILogger<RecordUserSignInCommand> _logger;

    public RecordUserSignInCommand(IUsersDbContext db, ICreateUserCommand createUserCommand, ILogger<RecordUserSignInCommand> logger = null)
    {
        _db = db;
        _createUserCommand = createUserCommand;
        _logger = logger;
    }

    public async Task<int> RecordSignIn(RecordUserSignInDto recordUserSignInDto)
    {
        if (recordUserSignInDto is null)
        {
            _logger.LogWarning($"dto object is null");
            return 0;
        }

        _logger?.LogWarning($"Dfe signin ref is {recordUserSignInDto.DfeSignInRef}");
        //Check user exists already
        var getUserIdQuery = new GetUserIdQuery(_db);
        var existingUserId = await getUserIdQuery.GetUserId(recordUserSignInDto.DfeSignInRef);

        _logger?.LogWarning($"existing user id is {existingUserId}");
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

    private async Task<int> AddSignInDetails(Domain.SignIn.Models.SignIn signIn) {
        _db.AddSignIn(signIn);
        var signInId = await _db.SaveChangesAsync();

        return signInId;
    }
}