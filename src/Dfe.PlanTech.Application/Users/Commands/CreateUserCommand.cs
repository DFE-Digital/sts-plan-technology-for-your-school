using Dfe.PlanTech.Domain.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Application.Users.Commands;

public class CreateUserCommand : ICreateUserCommand
{
    private readonly IPlanTechDbContext _db;

    public CreateUserCommand(IPlanTechDbContext db)
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
            DfeSignInRef = createUserDTO.DfeSignInRef,
            DateCreated = DateTime.UtcNow
        };

        _db.AddUser(user);

        await _db.SaveChangesAsync();

        return user.Id;
    }
}

