using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Application.Persistence.Commands;

public class CreateUserCommand
{
    private readonly IUsersDbContext _db;

    public CreateUserCommand(IUsersDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Creates new user and returns ID
    /// </summary>
    /// <param name="createUserDTO"></param>
    /// <returns></returns>
    public async Task<int> CreateUser(RecordUserSignInDto createUserDTO)
    {
        var user = new User()
        {
            DfeSignInRef = createUserDTO.DfeSignInRef
        };

        _db.AddUser(user);

        await _db.SaveChangesAsync();

        return user.Id;
    }
}

