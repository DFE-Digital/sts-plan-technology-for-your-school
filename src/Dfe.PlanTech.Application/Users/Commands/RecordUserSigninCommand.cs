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
        var signIn = await CreateSignIn(recordUserSignInDto);

        var signInId = await AddSignInDetails(signIn);

        return signInId;
    }

    /// <summary>
    /// Creates SignIn row
    /// </summary>
    /// <param name="recordUserSignInDto"></param>
    /// <returns></returns>
    private async Task<Domain.SignIn.Models.SignIn> CreateSignIn(RecordUserSignInDto recordUserSignInDto)
    {
        var getUserIdQuery = new GetUserIdQuery(_db);
        int userId = await GetUserId(recordUserSignInDto, getUserIdQuery);
        var signIn = MapToSignIn(userId);

        return signIn;
    }

    /// <summary>
    /// Gets existing ID for user, or creates a new user in database if non existing
    /// </summary>
    /// <param name="recordUserSignInDto"></param>
    /// <param name="getUserIdQuery"></param>
    /// <returns></returns>
    private async Task<int> GetUserId(RecordUserSignInDto recordUserSignInDto, GetUserIdQuery getUserIdQuery)
    {
        var existingUserId = await getUserIdQuery.GetUserId(recordUserSignInDto.DfeSignInRef);

        if (existingUserId != null)
        {
            return existingUserId.Value;
        }

        return await _createUserCommand.CreateUser(recordUserSignInDto);
    }

    private static Domain.SignIn.Models.SignIn MapToSignIn(int userId, int establishmentId = 1)
    {
        if (userId == 0)
            throw new ArgumentNullException(nameof(userId), "User id cannot be null");

        return new Domain.SignIn.Models.SignIn
        {
            UserId = Convert.ToUInt16(userId),
            EstablishmentId = establishmentId,
            SignInDateTime = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Record SignIn in database
    /// </summary>
    /// <param name="signIn"></param>
    /// <returns></returns>
    private async Task<int> AddSignInDetails(Domain.SignIn.Models.SignIn signIn)
    {
        _db.AddSignIn(signIn);
        var signInId = await _db.SaveChangesAsync();

        return signInId;
    }
}